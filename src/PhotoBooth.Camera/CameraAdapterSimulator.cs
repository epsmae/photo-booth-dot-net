using System;
using System.IO;
using System.Threading.Tasks;
using PhotoBooth.Abstraction;

namespace PhotoBooth.Camera
{
    public class CameraAdapterSimulator : ICameraAdapter
    {
        public Task<CommandLineResult> Capture(string selectedCamera, string fileName)
        {
            string directory = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.Copy("SampleImage.jpg", fileName);

            return Task.FromResult(CreateSuccessResult());
        }

        public async Task<CommandLineResult> ListCameras()
        {
            await Task.Delay(200);
            return CreateSuccessResult($"Model Port {Environment.NewLine}---{Environment.NewLine}Nikon  Port");
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

        public async Task<CommandLineResult> Configure()
        {
            await Task.Delay(20);
            return CreateSuccessResult();
        }

        private static CommandLineResult CreateSuccessResult()
        {
            return CreateSuccessResult(string.Empty);
        }

        private static CommandLineResult CreateSuccessResult(string standardOutput)
        {
            return new CommandLineResult
            {
                StandardError = string.Empty,
                StandardOutput = standardOutput,
                ExitCode = 0
            };
        }
    }
}
