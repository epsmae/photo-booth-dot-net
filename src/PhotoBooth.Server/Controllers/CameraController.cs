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
    public class CameraController : ControllerBase
    {
        private readonly ILogger<CaptureController> _logger;
        private ICameraService _cameraService;

        public CameraController(ILogger<CaptureController> logger, ICameraService cameraService)
        {
            _logger = logger;
            _cameraService = cameraService;
        }

        [HttpGet]
        [ActionName(nameof(Cameras))]
        public Task<List<CameraInfo>> Cameras()
        {
            return _cameraService.ListCameras();
        }
    }
}
