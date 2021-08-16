using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PhotoBooth.Abstraction;
using System;
using System.Threading.Tasks;
using PhotoBooth.Abstraction.Exceptions;

namespace PhotoBooth.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class CaptureController : ControllerBase
    {
        private readonly ILogger<CaptureController> _logger;
        private readonly IWorkflowController _workflowController;

        public CaptureController(ILogger<CaptureController> logger, IWorkflowController workflowController)
        {
            _logger = logger;
            _workflowController = workflowController;
        }

        [HttpPost]
        [ActionName(nameof(Capture))]
        public async Task Capture()
        {
            try
            {
                await _workflowController.Capture();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to trigger capture");
            }
        }


        [HttpPost]
        [ActionName(nameof(Print))]
        public async Task Print()
        {
            try
            {
                await _workflowController.Print();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to trigger print");
            }
        }

        [HttpPost]
        [ActionName(nameof(ConfirmError))]
        public async Task ConfirmError()
        {
            try
            {
                await _workflowController.ConfirmError();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to confirm error");
            }
        }


        [HttpPost]
        [ActionName(nameof(Next))]
        public async Task Next()
        {
            try
            {
                await _workflowController.Capture();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to trigger next");
            }
        }

        [HttpGet]
        [ActionName(nameof(State))]
        public CaptureProcessState State()
        {

            return _workflowController.State;
        }

        [HttpGet]
        [ActionName(nameof(LastException))]
        public CaptureError LastException()
        {
            return new CaptureError
            {
                Exception = ExceptionHelper.Convert(_workflowController.LastException),
                ErrorMessage = _workflowController.LastException.Message
            };
        }

        [HttpGet]
        [ActionName(nameof(CountDownStep))]
        public int CountDownStep()
        {

            return _workflowController.CurrentCountDownStep;
        }


        [HttpGet]
        [ActionName(nameof(Image))]
        public string Image()
        {

            return _workflowController.CurrentImageFileName;
        }

        [HttpGet]
        [ActionName(nameof(ImageData))]
        public byte[] ImageData()
        {

            _logger.LogInformation($"Getting image data length={_workflowController.ImageData?.Length}");

            return _workflowController?.ImageData;
        }
    }
}
