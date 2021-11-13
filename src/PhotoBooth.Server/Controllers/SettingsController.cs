using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using PhotoBooth.Abstraction;
using PhotoBooth.Abstraction.Configuration;

namespace PhotoBooth.Server.Controllers
{
    [Controller]
    [Route("api/[controller]/[action]")]
    public class SettingsController : ControllerBase
    {
        private readonly IConfigurationService _service;
        private readonly IFileService _fileService;

        public SettingsController(IConfigurationService service, IFileService fileService)
        {
            _service = service;
            _fileService = fileService;
        }
        
        [HttpGet]
        [ActionName(nameof(Settings))]
        public SettingsDto Settings()
        {
            return new SettingsDto
            {
                CaptureCountDownStepCount = _service.CaptureCountDownStepCount,
                ReviewCountDownStepCount = _service.ReviewCountDownStepCount,
                StepDownDurationInSeconds = _service.StepDownDurationInSeconds,
                ReviewImageWidth = _service.ReviewImageWidth,
                SelectedCamera = _service.SelectedCamera,
                SelectedPrinter = _service.SelectedPrinter,
                ReviewImageQuality = _service.ReviewImageQuality
            };
        }

        [HttpPost]
        [ActionName(nameof(SetSettings))]
        public void SetSettings([FromBody] SettingsDto settings)
        {
            _service.ReviewCountDownStepCount = settings.ReviewCountDownStepCount;
            _service.CaptureCountDownStepCount = settings.CaptureCountDownStepCount;
            _service.StepDownDurationInSeconds = settings.StepDownDurationInSeconds;
            _service.ReviewImageWidth = settings.ReviewImageWidth;
            _service.SelectedCamera = settings.SelectedCamera;
            _service.SelectedPrinter = settings.SelectedPrinter;
            _service.ReviewImageQuality = settings.ReviewImageQuality;
        }

        [HttpGet]
        [ActionName(nameof(AvailableImages))]
        public List<FileInfo> AvailableImages()
        {
            return _fileService.AvailableImages;
        }

        [HttpGet]
        [ActionName(nameof(PrintServerUrl))]
        public string PrintServerUrl()
        {
            return $"http://localhost:631";
        }


        [HttpPost]
        [ActionName(nameof(ClearImages))]
        public void ClearImages()
        {
            _fileService.CleanupImageDirectory();
        }
    }
}
