using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PhotoBooth.Abstraction;

namespace PhotoBooth.Server
{
    public class CaptureHub : Hub
    {
        private ILogger<CaptureHub> _logger;
        private readonly ICameraService _cameraService;
        private readonly IPrinterService _printerService;
        private readonly IFileService _fileService;
        private readonly IImageResizer _imageResizer;

        public CaptureHub(ILogger<CaptureHub> logger, ICameraService cameraService, IPrinterService printerService, IFileService fileService, IImageResizer imageResizer)
        {
            _logger = logger;
            _cameraService = cameraService;
            _printerService = printerService;
            _fileService = fileService;
            _imageResizer = imageResizer;
        }


        public async Task SendMessage(string user, string message)
        {
            await Clients?.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task SendStateChanged(CaptureState state)
        {
            if (Clients != null && Clients.All  != null)
            {
                await Clients?.All.SendAsync("ReceiveStateChanged", state);
            }
        }

        public async Task SendCountDownStepChanged(int step)
        {
            if (Clients != null && Clients.All != null)
            {
                await Clients?.All.SendAsync("ReceiveCountDownStepChanged", step);
            }
        }

        public async Task SendReviewCountDownStepChanged(int step)
        {
            if (Clients != null && Clients.All != null)
            {
                await Clients?.All.SendAsync("ReceiveReviewCountDownStepChanged", step);
            }
        }

        public async Task<string> CaptureImage(string camera)
        {
            CaptureResult result = await _cameraService.CaptureImage(_fileService.PhotoDirectory, camera);
            return result.FileName;
        }

        public async Task<PreviewCaptureResult> CaptureImageData(string camera)
        {
            try
            {
                await _cameraService.Initialize();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize, ignore");
            }

            try
            {
                await _cameraService.Configure();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to configure, ignore");
            }
                        
            PreviewCaptureResult previewCaptureResult = new PreviewCaptureResult();

            CaptureResult result = await _cameraService.CaptureImage(_fileService.PhotoDirectory, camera);
            previewCaptureResult.FileName = result.FileName;

            using (Stream stream = _fileService.OpenFile(result.FileName))
            {
                previewCaptureResult.ThumbnailData = _imageResizer.ResizeImage(stream, 256, 30);
            }

            return previewCaptureResult;
        }

        public async Task PrintImage(string printer, string fileName)
        {
            string fullImagePath = Path.Combine(_fileService.PhotoDirectory, fileName);
            await _printerService.Print(printer, fullImagePath);
        }
    }
}
