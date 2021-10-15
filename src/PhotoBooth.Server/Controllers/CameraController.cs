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
        private readonly IFileService _fileService;

        public CameraController(ILogger<CaptureController> logger, ICameraService cameraService, IFileService fileService)
        {
            _logger = logger;
            _cameraService = cameraService;
            _fileService = fileService;
        }

        [HttpGet]
        [ActionName(nameof(Cameras))]
        public Task<List<CameraInfo>> Cameras()
        {
            return _cameraService.ListCameras();
        }

        [HttpGet]
        [ActionName(nameof(CaptureImageData))]
        public Task<byte[]> CaptureImageData(string camera)
        {
            return _cameraService.CaptureImageData(_fileService.PhotoDirectory, camera);
        }
    }
}
