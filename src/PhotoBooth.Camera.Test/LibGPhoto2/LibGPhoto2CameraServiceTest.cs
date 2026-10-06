using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using PhotoBooth.Abstraction;
using PhotoBooth.Abstraction.Exceptions;
using PhotoBooth.Abstraction.LibGPhoto2;
using PhotoBooth.Abstraction.LiveView;
using PhotoBooth.Camera.LibGPhoto2;

namespace PhotoBooth.Camera.Test.LibGPhoto2
{
    public class LibGPhoto2CameraServiceTest
    {
        [Test]
        public void TestExceptionMapping()
        {
            Assert.IsInstanceOf<CameraNotAvailableException>(Map(GPhoto2Exception.ErrorModelNotFound, "gp_camera_init failed: Unknown model (-105)"));
            Assert.IsInstanceOf<CameraClaimException>(Map(GPhoto2Exception.ErrorIoUsbClaim, "Could not claim the USB device"));
            Assert.IsInstanceOf<CameraOutOfFocusException>(Map(GPhoto2Exception.ErrorGeneric, "gp_camera_capture failed: Unspecified error (-1): Out of Focus"));
            Assert.IsInstanceOf<PtpStoreException>(Map(GPhoto2Exception.ErrorGeneric, "PTP Store Not Available"));
            Assert.IsInstanceOf<CameraException>(Map(GPhoto2Exception.ErrorGeneric, "something else"));
        }

        [Test]
        public async Task TestListCamerasWithoutCamera()
        {
            FakeGPhoto2Api api = new FakeGPhoto2Api {ConnectFailure = () => new GPhoto2Exception(GPhoto2Exception.ErrorModelNotFound, "no camera")};
            using CameraSession session = CreateSession(api);
            LibGPhoto2CameraService service = new LibGPhoto2CameraService(NullLogger<LibGPhoto2CameraService>.Instance, session);

            List<CameraInfo> cameras = await service.ListCameras();

            Assert.IsEmpty(cameras);
        }

        [Test]
        public async Task TestCapture()
        {
            FakeGPhoto2Api api = new FakeGPhoto2Api();
            using CameraSession session = CreateSession(api);
            LibGPhoto2CameraService service = new LibGPhoto2CameraService(NullLogger<LibGPhoto2CameraService>.Instance, session);

            CaptureResult result = await service.CaptureImage(System.IO.Path.GetTempPath(), "Fake Camera");

            FileAssert.Exists(result.FileName);
        }

        [Test]
        public async Task TestCaptureWithoutCamera()
        {
            FakeGPhoto2Api api = new FakeGPhoto2Api {ConnectFailure = () => new GPhoto2Exception(GPhoto2Exception.ErrorModelNotFound, "no camera")};
            using CameraSession session = CreateSession(api);
            LibGPhoto2CameraService service = new LibGPhoto2CameraService(NullLogger<LibGPhoto2CameraService>.Instance, session);

            await Assert.ThrowsAsync<CameraNotAvailableException>(async () => await service.CaptureImage(System.IO.Path.GetTempPath(), "Fake Camera"));
        }

        [Test]
        public async Task TestLiveViewSource()
        {
            FakeGPhoto2Api api = new FakeGPhoto2Api();
            using CameraSession session = CreateSession(api);
            LibGPhoto2LiveViewSource source = new LibGPhoto2LiveViewSource(session);
            Assert.True(source.SupportsCaptureDuringLiveView);

            int frames = 0;
            using System.Threading.CancellationTokenSource stop = new System.Threading.CancellationTokenSource();
            Task run = source.RunAsync(_ => System.Threading.Interlocked.Increment(ref frames), stop.Token, System.Threading.CancellationToken.None);

            while (System.Threading.Volatile.Read(ref frames) < 3)
            {
                await Task.Delay(10);
            }

            stop.Cancel();
            await Assert.ThrowsAsync(Is.InstanceOf<OperationCanceledException>(), async () => await run.WaitAsync(TimeSpan.FromSeconds(2)));
            Assert.False(session.IsPreviewing);
        }

        [Test]
        public async Task TestLiveViewSourceError()
        {
            FakeGPhoto2Api api = new FakeGPhoto2Api {PreviewFailure = _ => new GPhoto2Exception(GPhoto2Exception.ErrorGeneric, "Liveview cannot start: Temperature too high")};
            using CameraSession session = CreateSession(api, new CameraDriverOptions {MaxPreviewErrors = 1});
            LibGPhoto2LiveViewSource source = new LibGPhoto2LiveViewSource(session);

            LiveViewException exception = await Assert.ThrowsAsync<LiveViewException>(async () =>
                await source.RunAsync(_ => { }, System.Threading.CancellationToken.None, System.Threading.CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(5)));

            StringAssert.Contains("Temperature too high", exception.Message);
        }

        private static Exception Map(int code, string message)
        {
            return LibGPhoto2CameraService.MapException(new GPhoto2Exception(code, message));
        }

        private static CameraSession CreateSession(FakeGPhoto2Api api, CameraDriverOptions options = null)
        {
            return new CameraSession(api, NullLogger<CameraSession>.Instance, Options.Create(options ?? new CameraDriverOptions()));
        }
    }
}
