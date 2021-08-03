using System;
using System.Threading.Tasks;
using PhotoBooth.Abstraction;

namespace PhotoBooth.Camera
{
    public class CameraAdapterSimulator : ICameraAdapter
    {
        public async Task<CommandLineResult> Capture(string fileName)
        {
            await Task.Delay(5000);

            return CreateSuccessResult();
        }

        public async Task<CommandLineResult> ListCameras()
        {
            await Task.Delay(200);
            return CreateSuccessResult();
        }

        public async Task<CommandLineResult> GetStorageInfo()
        {
            await Task.Delay(200);
            return CreateSuccessResult();
        }

        public async Task<CommandLineResult> GetCameraStatus()
        {
            await Task.Delay(200);
            return CreateSuccessResult();
        }

        public async Task<CommandLineResult> GetBatteryInfo()
        {
            await Task.Delay(200);
            return CreateSuccessResult();
        }

        public async Task<CommandLineResult> Initialize()
        {
            await Task.Delay(200);
            return CreateSuccessResult();
        }

        private static CommandLineResult CreateSuccessResult()
        {
            return new CommandLineResult
            {
                StandardError = string.Empty,
                StandardOutput = string.Empty,
                ExitCode = 0
            };
        }
    }
}
