using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using PhotoBooth.Abstraction.LibGPhoto2;
using PhotoBooth.Camera.LibGPhoto2;

namespace PhotoBooth.Camera.Test.LibGPhoto2
{
    public class CameraSessionTest
    {
        private FakeGPhoto2Api _api;
        private CameraSession _session;
        private int _frames;

        [SetUp]
        public void Setup()
        {
            _api = new FakeGPhoto2Api();
            _frames = 0;
        }

        [TearDown]
        public void TearDown()
        {
            _session?.Dispose();
        }

        [Test]
        public async Task TestPreviewFrames()
        {
            CreateSession();
            _session.StartPreview(OnFrame, _ => { });

            await WaitFor(() => Frames >= 5);
            Assert.True(_session.IsInLiveView);

            _session.StopPreview();
            await Task.Delay(100);
            int frames = Frames;
            await Task.Delay(200);

            Assert.AreEqual(frames, Frames, "no frames after stop");
        }

        [Test]
        public async Task TestCaptureDuringLiveViewKeepsConnection()
        {
            CreateSession();
            _session.StartPreview(OnFrame, _ => { });
            await WaitFor(() => Frames >= 3);

            string file = TempFile();
            await _session.CaptureImageAsync(file);
            int framesAfterCapture = Frames;
            await WaitFor(() => Frames >= framesAfterCapture + 3);

            Assert.True(File.Exists(file));
            Assert.False(_api.Calls.Contains("Disconnect"), "no reconnect for a capture out of the live view");
            Assert.AreEqual(1, _api.Calls.Count(c => c == "Connect"));
            Assert.AreEqual(1, _api.ThreadIds.Count, "all camera calls on the session thread");

            int captureIndex = _api.Calls.IndexOf("CaptureImage");
            Assert.AreEqual("DownloadFile", _api.Calls[captureIndex + 1]);
            Assert.True(_api.Calls.Skip(captureIndex).Contains("CapturePreview"), "live view continues after the capture");
        }

        [Test]
        public async Task TestCaptureTargetIsConfiguredOnConnect()
        {
            CreateSession(new CameraDriverOptions {CaptureTarget = "Memory card"});
            await _session.ConnectAsync();

            CollectionAssert.AreEqual(new[] {"Connect", "GetModel", "SetConfig(capturetarget=Memory card)"}, _api.Calls);
        }

        [Test]
        public async Task TestLiveViewAutofocus()
        {
            CreateSession(new CameraDriverOptions {FocusMode = CameraFocusMode.LiveViewAutofocus});
            _session.StartPreview(OnFrame, _ => { });
            await WaitFor(() => Frames >= 2);

            await _session.CaptureImageAsync(TempFile());

            int captureIndex = _api.Calls.IndexOf("CaptureImage");
            Assert.AreEqual("SetConfig(autofocusdrive=1)", _api.Calls[captureIndex - 1]);
        }

        [Test]
        public async Task TestPhaseDetectLeavesLiveView()
        {
            CreateSession(new CameraDriverOptions {FocusMode = CameraFocusMode.PhaseDetect});
            _session.StartPreview(OnFrame, _ => { });
            await WaitFor(() => Frames >= 2);

            await _session.CaptureImageAsync(TempFile());

            int captureIndex = _api.Calls.IndexOf("CaptureImage");
            CollectionAssert.AreEqual(new[] {"Disconnect", "Connect", "GetModel", "SetConfig(capturetarget=Memory card)", "CaptureImage"},
                _api.Calls.Skip(captureIndex - 4).Take(5).ToList());
        }

        [Test]
        public async Task TestReconnectAfterConnectionError()
        {
            _api.PreviewFailure = n => n == 3 ? new GPhoto2Exception(GPhoto2Exception.ErrorIo, "I/O problem") : null;
            CreateSession();
            _session.StartPreview(OnFrame, _ => Assert.Fail("error must not be reported"));

            await WaitFor(() => Frames >= 5);

            Assert.AreEqual(1, _api.Calls.Count(c => c == "Disconnect"));
            Assert.AreEqual(2, _api.Calls.Count(c => c == "Connect"));
        }

        [Test]
        public async Task TestPreviewErrorIsReported()
        {
            _api.PreviewFailure = n => new GPhoto2Exception(GPhoto2Exception.ErrorGeneric, "Liveview cannot start: Battery exhausted");
            CreateSession(new CameraDriverOptions {MaxPreviewErrors = 2});

            TaskCompletionSource<Exception> error = new TaskCompletionSource<Exception>();
            _session.StartPreview(OnFrame, ex => error.TrySetResult(ex));

            Exception reported = await error.Task.WaitAsync(TimeSpan.FromSeconds(5));
            StringAssert.Contains("Battery exhausted", reported.Message);
            Assert.False(_session.IsPreviewing);
        }

        [Test]
        public async Task TestIdleEndsLiveView()
        {
            CreateSession(new CameraDriverOptions {LiveViewHoldSeconds = 1});
            _session.StartPreview(OnFrame, _ => { });
            await WaitFor(() => Frames >= 2);

            _session.StopPreview();
            Assert.True(_session.IsInLiveView, "camera stays in live view for a fast restart");

            await WaitFor(() => !_session.IsInLiveView);
            Assert.AreEqual("Disconnect", _api.Calls.Last());
        }

        [Test]
        public async Task TestNoCamera()
        {
            _api.ConnectFailure = () => new GPhoto2Exception(GPhoto2Exception.ErrorModelNotFound, "Could not detect any camera");
            CreateSession();

            GPhoto2Exception exception = await Assert.ThrowsAsync<GPhoto2Exception>(async () => await _session.CaptureImageAsync(TempFile()));
            Assert.AreEqual(GPhoto2Exception.ErrorModelNotFound, exception.Code);
        }

        private int Frames
        {
            get
            {
                return Volatile.Read(ref _frames);
            }
        }

        private void OnFrame(byte[] frame)
        {
            Interlocked.Increment(ref _frames);
        }

        private void CreateSession(CameraDriverOptions options = null)
        {
            _session = new CameraSession(_api, NullLogger<CameraSession>.Instance, Options.Create(options ?? new CameraDriverOptions()));
        }

        private static string TempFile()
        {
            return Path.Combine(Path.GetTempPath(), $"camera_session_test_{Guid.NewGuid():N}.jpg");
        }

        private static async Task WaitFor(Func<bool> condition)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            while (!condition())
            {
                if (stopwatch.Elapsed > TimeSpan.FromSeconds(10))
                {
                    Assert.Fail("Timeout");
                }

                await Task.Delay(10);
            }
        }
    }
}
