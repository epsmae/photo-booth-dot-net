using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using Microsoft.Extensions.Logging;
using PhotoBooth.Abstraction;
using PhotoBooth.Abstraction.Exceptions;

namespace PhotoBooth.Camera
{
    // For all available commands see:
    // http://manpages.ubuntu.com/manpages/bionic/man1/gphoto2.1.html
    public class CameraService : ICameraService
    {
        
        private readonly ILogger<CameraService> _logger;
        private readonly ICameraAdapter _adapter;

        public CameraService(ILogger<CameraService> logger, ICameraAdapter adapter)
        {
            _logger = logger;
            _adapter = adapter;
        }


        public async Task Initialize()
        {
            CommandLineResult result = await _adapter.Initialize();
            LogResult(result);
            EvaluateResult(result);
        }

        public async Task<byte[]> CaptureImageData()
        {
            CaptureResult result = await CaptureImage();

            return System.IO.File.ReadAllBytes(result.FileName);
        }

        public async Task<CaptureResult> CaptureImage()
        {
            string fileName = $"img_{System.DateTime.Now:dd-MM-yyyy_HH_mm_ss_fff}.jpg";

            _logger.LogInformation($"Capture image with file name={fileName}");

            CommandLineResult result = await _adapter.Capture(fileName);
            LogResult(result);
            EvaluateResult(result);

            return new CaptureResult
            {
                FileName = fileName
            };
        }
        
        public async Task<List<CameraInfo>> ListCameras()
        {
            _logger.LogInformation($"Listing cameras");

            CommandLineResult result = await _adapter.ListCameras();
            LogResult(result);
            EvaluateResult(result);


            string[] lines =
                result.StandardOutput.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);

            List<CameraInfo> cameras = new List<CameraInfo>();
            if (lines.Length > 2)
            {
                for (int i = 2; i < lines.Length; i++)
                {
                    string[] items = lines[i].Split(new[] {"  "}, StringSplitOptions.RemoveEmptyEntries);
                    if (items.Length >= 2)
                    {
                        cameras.Add(new CameraInfo
                        {
                            CameraModel = items[0].Trim(),
                            Port = items[1].Trim()
                        });
                    }
                }
            }
            
            return cameras;
        }


        public async Task<StorageInfo> FetchStorageInfo()
        {
            CommandLineResult result = await _adapter.GetStorageInfo();
            LogResult(result);
            EvaluateResult(result);



            //var result = await Cli.Wrap(GPhotoExe)
            //    .WithArguments("--storage-info")
            //    .ExecuteBufferedAsync();

         
            return new StorageInfo();
        }

        public async Task<CameraStatus> FetchCameraStatus()
        {
            CommandLineResult result = await _adapter.GetCameraStatus();
            LogResult(result);
            EvaluateResult(result);

            return new CameraStatus();
        }
        
        private void LogResult(CommandLineResult result)
        {
            _logger.LogInformation($"ExitCode: {result.ExitCode}");
            _logger.LogInformation($"StandardOutput: {result.StandardOutput}");
            _logger.LogInformation($"StandardError: {result.StandardError}");
        }

        private void EvaluateResult(CommandLineResult result)
        {
            if (result.StandardOutput.ToLower().Contains("Out of Focus".ToLower()))
            {
                throw new CameraException("Camera Out of Focus");
            }
            
            if (result.ExitCode != 0)
            {
                throw new CameraException(result.StandardError);
            }
        }
    }
}
