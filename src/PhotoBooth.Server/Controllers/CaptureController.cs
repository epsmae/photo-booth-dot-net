using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PhotoBooth.Abstraction;
using System;
using System.Threading.Tasks;

namespace PhotoBooth.Server.Controllers
{
    [ApiController]
    //[Route("[controller]")]
    [Route("api/[controller]/[action]")]
    public class CaptureController : ControllerBase
    {
        private readonly ILogger<CaptureController> _logger;
        private readonly IWorkflowController _workflowController;
        private ICameraService _cameraService;
        //private readonly LiveViewHelper liveViewHelper;

        public CaptureController(ILogger<CaptureController> logger, IWorkflowController workflowController, ICameraService cameraService)
        {
            _logger = logger;
            _workflowController = workflowController;
            _cameraService = cameraService;
            //this.liveViewHelper = liveViewHelper;
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


            //try
            //{
            //    _logger.LogInformation($"Capturing image");

            //    await _workflowController.Capture();

            //    return await _cameraService.CaptureImageData();
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "Failed to trigger capture");
            //}

            //return new byte[] { };
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

            return new CaptureError{ErrorMessage = _workflowController.LastException.Message};
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
