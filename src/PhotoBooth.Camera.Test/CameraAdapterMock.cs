using System;
using System.IO;
using Moq;
using NUnit.Framework;
using PhotoBooth.Abstraction;

namespace PhotoBooth.Camera.Test
{
    public class CameraAdapterMock
    {

        private readonly Mock<ICameraAdapter> _mock;

        private string SampleDirectory
        {
            get
            {
                return Path.Combine(TestContext.CurrentContext.TestDirectory, "SampleResponses");
            }
        }


        public CameraAdapterMock()
        {
            _mock = new Mock<ICameraAdapter>();
            _mock.Setup(m => m.Capture(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((string camera, string fileName) => Capture(camera, fileName));
            _mock.Setup(m => m.ListCameras()).ReturnsAsync(() => ListCameras());
        }

        private CommandLineResult ListCameras()
        {
            if (DoNotListCameras)
            {
                return CreateSuccessResult("list_cameras_not_available.txt");
            }

            return CreateSuccessResult("list_cameras_available.txt");
        }

        private CommandLineResult Capture(string camera, string fileName)
        {
            if (ThrowCaptureNocCameraError)
            {
                return CreateErrorResult("capture_error_no_camera.txt");
            }

            if (ThrowCaptureOutOfFocus)
            {
                return CreateSuccessResult("capture_error_out_of_focus.txt");
            }

            return CreateSuccessResult("capture_success.txt");
        }

        public ICameraAdapter Object
        {
            get
            {
                return _mock.Object;
            }
        }

        public bool ThrowCaptureNocCameraError
        {
            get;
            set;
        }

        public bool ThrowCaptureOutOfFocus
        {
            get;
            set;
        }

        public bool DoNotListCameras
        {
            get;
            set;
        }


        private CommandLineResult CreateSuccessResult(string responseFile)
        {
            return new CommandLineResult
            {
                StandardError = string.Empty,
                StandardOutput = File.ReadAllText(Path.Combine(SampleDirectory, responseFile)),
                ExitCode = 0
            };
        }

        private CommandLineResult CreateErrorResult(string responseFile)
        {
            return new CommandLineResult
            {
                StandardError = File.ReadAllText(Path.Combine(SampleDirectory, responseFile)),
                StandardOutput = string.Empty,
                ExitCode = 1
            };
        }
    }
}
