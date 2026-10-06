using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PhotoBooth.Abstraction.LibGPhoto2;

namespace PhotoBooth.Camera.LibGPhoto2
{
    /// <summary>
    /// Owns the camera connection (libgphoto2 is not thread safe) and runs all camera calls on one
    /// dedicated thread. While the live view is active, preview frames are fetched continuously;
    /// commands (capture, config, ...) are executed between two frames. The camera stays connected,
    /// so a capture needs no process start and no reconnect, and the live view continues right after
    /// the capture.
    /// </summary>
    public sealed class CameraSession : IDisposable
    {
        private const int IdleWaitMilliseconds = 250;
        private const int BusyRetryMilliseconds = 10;
        private const int ErrorRetryMilliseconds = 500;

        private readonly IGPhoto2Api _api;
        private readonly ILogger<CameraSession> _logger;
        private readonly CameraDriverOptions _options;
        private readonly BlockingCollection<WorkItem> _queue = new BlockingCollection<WorkItem>();
        private readonly Thread _thread;
        private readonly CancellationTokenSource _shutdown = new CancellationTokenSource();
        private readonly object _previewLock = new object();

        private Action<byte[]> _previewHandler;
        private Action<Exception> _previewErrorHandler;
        private int _consecutivePreviewErrors;
        private bool _inLiveView;
        private readonly Stopwatch _sinceLastPreview = new Stopwatch();
        private int _sessionThreadId;

        public CameraSession(IGPhoto2Api api, ILogger<CameraSession> logger, IOptions<CameraDriverOptions> options)
        {
            _api = api;
            _logger = logger;
            _options = options.Value;
            _thread = new Thread(Run) {IsBackground = true, Name = "CameraSession"};
            _thread.Start();
        }

        public bool IsPreviewing
        {
            get
            {
                lock (_previewLock)
                {
                    return _previewHandler != null;
                }
            }
        }

        /// <summary>
        /// Camera is in live view (mirror up). Only updated on the session thread.
        /// </summary>
        public bool IsInLiveView
        {
            get
            {
                return Volatile.Read(ref _inLiveView);
            }
        }

        public Task ConnectAsync()
        {
            return InvokeAsync(api => true);
        }

        public Task<string> GetModelAsync()
        {
            return InvokeAsync(api => api.GetModel());
        }

        public Task SetConfigAsync(string name, string value)
        {
            return InvokeAsync(api =>
            {
                api.SetConfig(name, value);
                return true;
            });
        }

        /// <summary>
        /// Captures an image (out of the live view if it is running) and downloads it to <paramref name="targetFile"/>.
        /// </summary>
        public Task<CameraFileLocation> CaptureImageAsync(string targetFile)
        {
            return InvokeAsync(api =>
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                bool wasInLiveView = _inLiveView;

                PrepareFocus(api);
                long focusMs = stopwatch.ElapsedMilliseconds;

                CameraFileLocation location = api.CaptureImage();
                long captureMs = stopwatch.ElapsedMilliseconds;

                api.DownloadFile(location, targetFile);

                _logger.LogInformation($"Captured {location} -> {targetFile} (live view={wasInLiveView}, focus={_options.FocusMode}: {focusMs}ms, capture: {captureMs - focusMs}ms, download: {stopwatch.ElapsedMilliseconds - captureMs}ms)");
                return location;
            });
        }

        public void StartPreview(Action<byte[]> onFrame, Action<Exception> onError)
        {
            lock (_previewLock)
            {
                _previewHandler = onFrame;
                _previewErrorHandler = onError;
                _consecutivePreviewErrors = 0;
            }

            Wake();
        }

        public void StopPreview()
        {
            lock (_previewLock)
            {
                _previewHandler = null;
                _previewErrorHandler = null;
            }

            Wake();
        }

        public void Dispose()
        {
            if (_shutdown.IsCancellationRequested)
            {
                return;
            }

            _shutdown.Cancel();
            _queue.CompleteAdding();

            if (!_thread.Join(TimeSpan.FromSeconds(10)))
            {
                _logger.LogWarning("Camera session thread did not stop");
            }

            _api.Dispose();
        }

        private Task<T> InvokeAsync<T>(Func<IGPhoto2Api, T> func)
        {
            WorkItem<T> item = new WorkItem<T>(func);

            try
            {
                _queue.Add(item);
            }
            catch (InvalidOperationException)
            {
                return Task.FromException<T>(new ObjectDisposedException(nameof(CameraSession)));
            }

            return item.Task;
        }

        private void Wake()
        {
            try
            {
                _queue.Add(WorkItem.Noop);
            }
            catch (InvalidOperationException)
            {
                // disposed
            }
        }

        private void Run()
        {
            _sessionThreadId = Environment.CurrentManagedThreadId;

            try
            {
                while (!_shutdown.IsCancellationRequested)
                {
                    bool previewing = IsPreviewing;

                    // commands have priority over preview frames
                    if (_queue.TryTake(out WorkItem item, previewing ? 0 : IdleWaitMilliseconds))
                    {
                        Execute(item);
                        continue;
                    }

                    if (previewing)
                    {
                        FetchPreviewFrame();
                    }
                    else
                    {
                        EndLiveViewIfIdle();
                    }
                }
            }
            catch (Exception ex) when (ex is OperationCanceledException || ex is InvalidOperationException)
            {
                // shutdown
            }
            finally
            {
                foreach (WorkItem pending in _queue)
                {
                    pending.Fail(new ObjectDisposedException(nameof(CameraSession)));
                }

                SafeDisconnect();
            }
        }

        private void Execute(WorkItem item)
        {
            if (item == WorkItem.Noop)
            {
                return;
            }

            try
            {
                EnsureConnected();
                item.Execute(_api);
            }
            catch (Exception ex)
            {
                HandleCameraError(ex);
                item.Fail(ex);
            }
        }

        private void FetchPreviewFrame()
        {
            try
            {
                EnsureConnected();
                byte[] frame = _api.CapturePreview();

                _inLiveView = true;
                _sinceLastPreview.Restart();
                _consecutivePreviewErrors = 0;

                Action<byte[]> handler;

                lock (_previewLock)
                {
                    handler = _previewHandler;
                }

                handler?.Invoke(frame);
            }
            catch (GPhoto2Exception ex) when (ex.IsBusy)
            {
                Thread.Sleep(BusyRetryMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Preview failed: {ex.Message}");
                HandleCameraError(ex);

                Action<Exception> errorHandler = null;

                lock (_previewLock)
                {
                    _consecutivePreviewErrors++;

                    if (_consecutivePreviewErrors >= _options.MaxPreviewErrors)
                    {
                        errorHandler = _previewErrorHandler;
                        _previewHandler = null;
                        _previewErrorHandler = null;
                    }
                }

                if (errorHandler != null)
                {
                    errorHandler(ex);
                }
                else
                {
                    _shutdown.Token.WaitHandle.WaitOne(ErrorRetryMilliseconds);
                }
            }
        }

        private void PrepareFocus(IGPhoto2Api api)
        {
            switch (_options.FocusMode)
            {
                case CameraFocusMode.LiveViewAutofocus:
                    if (_inLiveView)
                    {
                        try
                        {
                            // Nikon contrast autofocus in live view
                            api.SetConfig("autofocusdrive", "1");
                        }
                        catch (GPhoto2Exception ex)
                        {
                            _logger.LogWarning($"Live view autofocus failed, capturing anyway: {ex.Message}");
                        }
                    }

                    break;

                case CameraFocusMode.PhaseDetect:
                    if (_inLiveView)
                    {
                        // gp_camera_exit ends the live view (mirror down), libgphoto2 then captures with autofocus
                        _logger.LogInformation("Leaving live view for phase detection autofocus");
                        SafeDisconnect();
                        EnsureConnected();
                    }

                    break;
            }
        }

        private void EndLiveViewIfIdle()
        {
            if (_inLiveView && _sinceLastPreview.Elapsed >= TimeSpan.FromSeconds(_options.LiveViewHoldSeconds))
            {
                _logger.LogInformation($"No preview for {_options.LiveViewHoldSeconds}s, ending live view on the camera");
                SafeDisconnect();
            }
        }

        private void EnsureConnected()
        {
            if (_api.IsConnected)
            {
                return;
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            _api.Connect();
            _inLiveView = false;

            string model = TryGetModel();
            _logger.LogInformation($"Camera connected in {stopwatch.ElapsedMilliseconds}ms: {model}");

            if (!string.IsNullOrEmpty(_options.CaptureTarget))
            {
                try
                {
                    _api.SetConfig("capturetarget", _options.CaptureTarget);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to set capturetarget={_options.CaptureTarget}: {ex.Message}");
                }
            }
        }

        private string TryGetModel()
        {
            try
            {
                return _api.GetModel();
            }
            catch (Exception ex)
            {
                return $"unknown model ({ex.Message})";
            }
        }

        private void HandleCameraError(Exception ex)
        {
            if (ex is GPhoto2Exception gphotoException && gphotoException.IsConnectionError)
            {
                _logger.LogWarning($"Camera connection lost: {ex.Message}");
                SafeDisconnect();
            }
        }

        private void SafeDisconnect()
        {
            try
            {
                _api.Disconnect();
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Disconnect failed: {ex.Message}");
            }

            _inLiveView = false;
        }

        internal int SessionThreadId
        {
            get
            {
                return _sessionThreadId;
            }
        }

        private abstract class WorkItem
        {
            public static readonly WorkItem Noop = new WorkItem<bool>(api => true);

            public abstract void Execute(IGPhoto2Api api);

            public abstract void Fail(Exception ex);
        }

        private sealed class WorkItem<T> : WorkItem
        {
            private readonly Func<IGPhoto2Api, T> _func;
            private readonly TaskCompletionSource<T> _completion = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

            public WorkItem(Func<IGPhoto2Api, T> func)
            {
                _func = func;
            }

            public Task<T> Task
            {
                get
                {
                    return _completion.Task;
                }
            }

            public override void Execute(IGPhoto2Api api)
            {
                _completion.TrySetResult(_func(api));
            }

            public override void Fail(Exception ex)
            {
                _completion.TrySetException(ex);
            }
        }
    }
}
