using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PhotoBooth.Abstraction;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoBooth.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class SetupController : ControllerBase
    {
        private readonly ILogger<CaptureController> _logger;
        private readonly IPrinterService _printerService;
        private readonly ICameraService _cameraService;

        public SetupController(ILogger<CaptureController> logger, IPrinterService printerService, ICameraService cameraService)
        {
            _logger = logger;
            _printerService = printerService;
            _cameraService = cameraService;
        }
    }
}
