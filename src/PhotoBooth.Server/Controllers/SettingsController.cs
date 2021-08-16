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

        public SettingsController(IConfigurationService service)
        {
            _service = service;
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
                SelectedPrinter = _service.SelectedPrinter
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
        }
    }
}
