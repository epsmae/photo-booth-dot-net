using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PhotoBooth.Abstraction;

namespace PhotoBooth.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class SetupController : ControllerBase
    {
        private readonly ILogger<CaptureController> _logger;
        private readonly IPrinterService _printerService;
        private readonly ICameraService _cameraService;
        private readonly IUsbService _usbService;

        public SetupController(ILogger<CaptureController> logger, IPrinterService printerService, ICameraService cameraService, IUsbService usbService)
        {
            _logger = logger;
            _printerService = printerService;
            _cameraService = cameraService;
            _usbService = usbService;
        }

        [HttpGet]
        [ActionName(nameof(GetUsbDevices))]
        public Task<List<string>> GetUsbDevices()
        {
            return _usbService.ListUsbDevices();
        }
    }
}
