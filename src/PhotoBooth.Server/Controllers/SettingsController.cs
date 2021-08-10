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
                StepDownDurationInSeconds = _service.StepDownDurationInSeconds
            };
        }

        [HttpPost]
        [ActionName(nameof(SetSettings))]
        public void SetSettings([FromBody] SettingsDto settings)
        {
            _service.ReviewCountDownStepCount = settings.ReviewCountDownStepCount;
            _service.CaptureCountDownStepCount = settings.CaptureCountDownStepCount;
            _service.StepDownDurationInSeconds = settings.StepDownDurationInSeconds;
        }

        [HttpGet]
        [ActionName(nameof(CaptureCountDownStepCount))]
        public int CaptureCountDownStepCount()
        {
            return _service.CaptureCountDownStepCount;
        }

        [HttpGet]
        [ActionName(nameof(ReviewCountDownStepCount))]
        public int ReviewCountDownStepCount()
        {
            return _service.ReviewCountDownStepCount;
        }

        [HttpGet]
        [ActionName(nameof(StepDownDurationInSeconds))]
        public double StepDownDurationInSeconds()
        {
            return _service.StepDownDurationInSeconds;
        }



        [HttpPost]
        [ActionName(nameof(SetCaptureCountDownStepCount))]
        public void SetCaptureCountDownStepCount(int value)
        {
            _service.CaptureCountDownStepCount = value;
        }

        [HttpPost]
        [ActionName(nameof(SetReviewCountDownStepCount))]
        public void SetReviewCountDownStepCount(int value)
        {
            _service.ReviewCountDownStepCount = value;
        }

        [HttpPost]
        [ActionName(nameof(SetStepDownDurationInSeconds))]
        public void SetStepDownDurationInSeconds(double value)
        {
            _service.StepDownDurationInSeconds = value;
        }

    }
}
