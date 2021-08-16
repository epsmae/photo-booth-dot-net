using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using PhotoBooth.Abstraction;
using PhotoBooth.Abstraction.Exceptions;

namespace PhotoBooth.Camera.Test
{
    public class CameraServiceTest
    {
        private readonly string _imageDirectory = System.IO.Path.Combine(TestContext.CurrentContext.WorkDirectory, "Images");
        private CameraAdapterMock _mock;
        private CameraService _cameraService;

        [SetUp]
        public void Setup()
        {

            _mock = new CameraAdapterMock();
            _cameraService = new CameraService(NullLogger<CameraService>.Instance, _mock.Object);
        }

        [Test]
        public async Task TestCaptureSuccessful()
        {
            CaptureResult result = await _cameraService.CaptureImage(_imageDirectory, "my_camera");
            Assert.False(string.IsNullOrEmpty(result.FileName));
        }

        [Test]
        public void TestCaptureNoCameraError()
        {
            _mock.ThrowCaptureNocCameraError = true;

            Assert.CatchAsync<CameraException>(async () => await _cameraService.CaptureImage(_imageDirectory,"my_camera"));
        }

        [Test]
        public void TestCaptureOutOfFocusError()
        {
            _mock.ThrowCaptureOutOfFocus = true;

            Assert.CatchAsync<CameraException>(async () => await _cameraService.CaptureImage(_imageDirectory, "my_camera"));
        }

        [Test]
        public async Task TestListCamerasSuccessful()
        {
            List<CameraInfo> cameras = await _cameraService.ListCameras();

            Assert.AreEqual(2, cameras.Count);
            for (int i = 0; i < 2; i++)
            {
                Assert.AreEqual(cameras[i].CameraModel, "Nikon DSC D7000 (PTP mode)");
                Assert.AreEqual(cameras[i].Port, "usb:001,007");
            }
        }

        [Test]
        public async Task TestListCamerasNone()
        {
            _mock.DoNotListCameras = true;

            List<CameraInfo> cameras = await _cameraService.ListCameras();

            Assert.AreEqual(0, cameras.Count);
        }
    }
}