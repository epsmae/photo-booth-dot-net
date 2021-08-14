using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PhotoBooth.Abstraction;


namespace PhotoBooth.Console
{
    public class CameraCommandHandler
    {
        private readonly ILogger<PrintCommandHandler> _logger;
        private readonly ICameraService _service;

        public CameraCommandHandler(ILogger<PrintCommandHandler> logger, ICameraService service)
        {
            _logger = logger;
            _service = service;
        }
        
        public IList<Command> BuildPrintCommand()
        {
            IList<Command> items = new List<Command>();

            Command captureCommand = new Command("capture");
            captureCommand.Handler = CommandHandler.Create(async() => await Capture());
            items.Add(captureCommand);

            Command listCameras = new Command("listCameras");
            listCameras.Handler = CommandHandler.Create(async () => await ListCameras());
            items.Add(listCameras);

            Command cameraStatus = new Command("cameraStatus");
            cameraStatus.Handler = CommandHandler.Create(async () => await FetchCameraStatus());
            items.Add(cameraStatus);

            return items;
        }

        private async Task<int> FetchCameraStatus()
        {
            try
            {
                CameraStatus cameraStatus = await _service.FetchCameraStatus();

                _logger.LogInformation($"{cameraStatus}");
                return ResultCodes.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch camera Status");
                return ResultCodes.Error;
            }
        }

        private async Task<int> ListCameras()
        {
            try
            {
                List<CameraInfo> cameras = await _service.ListCameras();

                foreach (CameraInfo camera in cameras)
                {
                    _logger.LogInformation($"{camera.CameraModel}, {camera.Port}");
                }
                return ResultCodes.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list cameras");
                return ResultCodes.Error;
            }
        }

        private async Task<int> Capture()
        {
            try
            {
                List<CameraInfo> cameras = await _service.ListCameras();

                CaptureResult result = await _service.CaptureImage(cameras.First().CameraModel);
                _logger.LogInformation($"Capture result: {result.FileName}");
                return ResultCodes.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to capture");
                return ResultCodes.Error;
            }
        }
    }
}
