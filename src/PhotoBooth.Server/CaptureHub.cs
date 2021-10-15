using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using PhotoBooth.Abstraction;

namespace PhotoBooth.Server
{
    public class CaptureHub : Hub
    {
        private readonly ICameraService _cameraService;
        private readonly IPrinterService _printerService;
        private readonly IFileService _fileService;

        public CaptureHub(ICameraService cameraService, IPrinterService printerService, IFileService fileService)
        {
            _cameraService = cameraService;
            _printerService = printerService;
            _fileService = fileService;
        }


        public async Task SendMessage(string user, string message)
        {
            await Clients?.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task SendStateChanged(CaptureProcessState state)
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

        public async Task PrintImage(string printer, string fileName)
        {
            string fullImagePath = Path.Combine(_fileService.PhotoDirectory, fileName);
            await _printerService.Print(printer, fullImagePath);
        }
    }
}
