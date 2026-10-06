using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PhotoBooth.Abstraction.LiveView;

namespace PhotoBooth.Service.LiveView
{
    /// <summary>
    /// Owns the live view source (camera), keeps the latest frame and hands it out to stream clients.
    /// Start and stop are serialized, <see cref="StopAsync"/> only returns once the camera is released.
    /// </summary>
    public class LiveViewService : ILiveViewService, IDisposable
    {
        public event EventHandler StatusChanged;

        private readonly ILogger<LiveViewService> _logger;
        private readonly ILiveViewSource _source;
        private readonly LiveViewOptions _options;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly Timer _idleTimer;
        private readonly object _fpsLock = new object();

        private CancellationTokenSource _stopCts;
        private CancellationTokenSource _killCts;
        private Task _runTask;
        private volatile bool _isRunning;
        private volatile LiveViewFrame _latestFrame;
        private TaskCompletionSource<LiveViewFrame> _nextFrame = CreateFrameCompletionSource();
        private long _frameId;
        private int _viewers;
        private string _lastError;

        private readonly Stopwatch _fpsStopwatch = Stopwatch.StartNew();
        private long _fpsWindowStartFrame;
        private double _fps;

        public LiveViewService(ILogger<LiveViewService> logger, ILiveViewSource source, IOptions<LiveViewOptions> options)
        {
            _logger = logger;
            _source = source;
            _options = options.Value;
            _idleTimer = new Timer(OnIdleTimeout, null, Timeout.Infinite, Timeout.Infinite);
        }

        public bool IsEnabled
        {
            get
            {
                return _options.Enabled;
            }
        }

        public bool IsRunning
        {
            get
            {
                return _isRunning;
            }
        }

        public LiveViewStatus GetStatus()
        {
            return new LiveViewStatus
            {
                Enabled = _options.Enabled,
                Running = _isRunning,
                Mirror = _options.Mirror,
                FrameCount = Interlocked.Read(ref _frameId),
                FramesPerSecond = _isRunning ? Math.Round(_fps, 1) : 0,
                Viewers = Volatile.Read(ref _viewers),
                LastError = _lastError
            };
        }

        public async Task StartAsync(Func<bool> canStart = null)
        {
            if (!_options.Enabled)
            {
                return;
            }

            await _lock.WaitAsync().ConfigureAwait(false);

            try
            {
                if (canStart != null && !canStart())
                {
                    return;
                }

                RestartIdleTimer();

                if (_runTask != null)
                {
                    if (!_runTask.IsCompleted)
                    {
                        return;
                    }

                    // the previous run ended by itself (e.g. camera error)
                    DisposeRun();
                }

                _lastError = null;
                _stopCts = new CancellationTokenSource();
                _killCts = new CancellationTokenSource();
                _isRunning = true;
                ResetFps();

                CancellationToken stopToken = _stopCts.Token;
                CancellationToken killToken = _killCts.Token;
                _runTask = Task.Run(() => Run(stopToken, killToken));
            }
            finally
            {
                _lock.Release();
            }

            NotifyStatusChanged();
        }

        public async Task StopAsync()
        {
            await _lock.WaitAsync().ConfigureAwait(false);

            try
            {
                _idleTimer.Change(Timeout.Infinite, Timeout.Infinite);

                if (_runTask == null)
                {
                    return;
                }

                Stopwatch stopwatch = Stopwatch.StartNew();
                _stopCts.Cancel();

                Task finished = await Task.WhenAny(_runTask, Task.Delay(_options.StopTimeoutMilliseconds)).ConfigureAwait(false);

                if (finished != _runTask)
                {
                    _logger.LogWarning($"Live view did not stop within {_options.StopTimeoutMilliseconds}ms, killing it");
                    _killCts.Cancel();
                    await Task.WhenAny(_runTask, Task.Delay(_options.StopTimeoutMilliseconds)).ConfigureAwait(false);
                }

                _logger.LogInformation($"Live view stopped in {stopwatch.ElapsedMilliseconds}ms");
                DisposeRun();
            }
            finally
            {
                _lock.Release();
            }

            NotifyStatusChanged();
        }

        public async Task<LiveViewFrame> WaitForFrameAsync(long afterFrameId, CancellationToken token)
        {
            // read the completion source before the latest frame, a frame is published by first
            // setting the latest frame and then completing the completion source
            TaskCompletionSource<LiveViewFrame> next = Volatile.Read(ref _nextFrame);
            LiveViewFrame latest = _latestFrame;

            if (latest != null && latest.Id > afterFrameId && _isRunning)
            {
                return latest;
            }

            if (!_isRunning)
            {
                return null;
            }

            return await next.Task.WaitAsync(token).ConfigureAwait(false);
        }

        public IDisposable RegisterViewer()
        {
            Interlocked.Increment(ref _viewers);
            return new Viewer(this);
        }

        public void Dispose()
        {
            _idleTimer.Dispose();
            _stopCts?.Cancel();
            _killCts?.Cancel();
        }

        private async Task Run(CancellationToken stopToken, CancellationToken killToken)
        {
            try
            {
                await _source.RunAsync(OnFrame, stopToken, killToken).ConfigureAwait(false);
                _logger.LogInformation("Live view source ended");
            }
            catch (OperationCanceledException) when (stopToken.IsCancellationRequested || killToken.IsCancellationRequested)
            {
                // requested stop
            }
            catch (Exception ex)
            {
                _lastError = ex.Message;
                _logger.LogError(ex, "Live view failed");
            }
            finally
            {
                _isRunning = false;

                // wake up the stream clients, they get null and close the stream
                Interlocked.Exchange(ref _nextFrame, CreateFrameCompletionSource()).TrySetResult(null);
            }

            if (!stopToken.IsCancellationRequested && !killToken.IsCancellationRequested)
            {
                // ended by itself, the next start cleans up
                NotifyStatusChanged();
            }
        }

        private void DisposeRun()
        {
            _stopCts.Dispose();
            _killCts.Dispose();
            _stopCts = null;
            _killCts = null;
            _runTask = null;
        }

        private void OnFrame(byte[] data)
        {
            long id = Interlocked.Increment(ref _frameId);
            LiveViewFrame frame = new LiveViewFrame(id, data);
            _latestFrame = frame;
            Interlocked.Exchange(ref _nextFrame, CreateFrameCompletionSource()).TrySetResult(frame);
            UpdateFps(id);
        }

        private void UpdateFps(long frameId)
        {
            lock (_fpsLock)
            {
                double elapsed = _fpsStopwatch.Elapsed.TotalSeconds;

                if (elapsed >= 1.0)
                {
                    _fps = (frameId - _fpsWindowStartFrame) / elapsed;
                    _fpsWindowStartFrame = frameId;
                    _fpsStopwatch.Restart();
                }
            }
        }

        private void ResetFps()
        {
            lock (_fpsLock)
            {
                _fps = 0;
                _fpsWindowStartFrame = Interlocked.Read(ref _frameId);
                _fpsStopwatch.Restart();
            }
        }

        private void RestartIdleTimer()
        {
            if (_options.IdleTimeoutSeconds > 0)
            {
                _idleTimer.Change(TimeSpan.FromSeconds(_options.IdleTimeoutSeconds), Timeout.InfiniteTimeSpan);
            }
        }

        private async void OnIdleTimeout(object state)
        {
            try
            {
                _logger.LogInformation($"Live view idle for {_options.IdleTimeoutSeconds}s, stopping it");
                await StopAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to stop idle live view");
            }
        }

        private void NotifyStatusChanged()
        {
            try
            {
                StatusChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to notify live view status changed");
            }
        }

        private static TaskCompletionSource<LiveViewFrame> CreateFrameCompletionSource()
        {
            return new TaskCompletionSource<LiveViewFrame>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        private sealed class Viewer : IDisposable
        {
            private LiveViewService _service;

            public Viewer(LiveViewService service)
            {
                _service = service;
            }

            public void Dispose()
            {
                LiveViewService service = Interlocked.Exchange(ref _service, null);

                if (service != null)
                {
                    Interlocked.Decrement(ref service._viewers);
                }
            }
        }
    }
}
