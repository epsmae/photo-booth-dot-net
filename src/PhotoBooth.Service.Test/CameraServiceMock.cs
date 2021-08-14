using Moq;
using PhotoBooth.Abstraction;
using System;
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
            _mock.Setup(m => m.CaptureImage(It.IsAny<string>())).Returns((string camera)=> Capture(camera));
        }

        private async Task<CaptureResult> Capture(string camera)
        {
            if (_throwCaptureException)
            {
                throw new Exception("Mock Camera Exception");
            }

            await Task.Delay(100);
            return new CaptureResult();
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
