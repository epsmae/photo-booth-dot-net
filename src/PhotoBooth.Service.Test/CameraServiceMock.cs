using Moq;
using PhotoBooth.Abstraction;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PhotoBooth.Service.Test
{
    internal class CameraServiceMock
    {
        private readonly Mock<ICameraService> _mock;
        private bool _throwCaptureException;

        internal CameraServiceMock()
        {
            _mock = new Mock<ICameraService>();
            _mock.Setup(m => m.CaptureImage(It.IsAny<string>(),It.IsAny<string>())).Returns((string directory, string camera)=> Capture(directory, camera));
        }

        private async Task<CaptureResult> Capture(string directory, string camera)
        {
            if (_throwCaptureException)
            {
                throw new Exception("Mock Camera Exception");
            }

            string fileName = Path.Combine(directory, $"img_{DateTime.Now:dd-MM-yyyy_HH_mm_ss_fff}.jpg");

            await Task.Delay(100);
            return new CaptureResult() {FileName = "SampleImage.jpg" };
        }

        internal ICameraService Object
        {
            get
            {
                return _mock.Object;
            }
        }

        public void ThrowCaptureException()
        {
            _throwCaptureException = true;
        }
    }
}
