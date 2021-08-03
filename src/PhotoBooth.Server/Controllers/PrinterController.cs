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
    public class PrinterController : ControllerBase
    {
        private readonly ILogger<CaptureController> _logger;
        private readonly IPrinterService _printerService;

        public PrinterController(ILogger<CaptureController> logger, IPrinterService printerService)
        {
            _logger = logger;
            _printerService = printerService;
        }

        [HttpGet]
        [ActionName(nameof(Printers))]
        public Task<List<Abstraction.Printer>> Printers()
        {
            return _printerService.ListPrinters();
        }

        [HttpGet]
        [ActionName(nameof(PrinterQueue))]
        public Task<List<PrintQueueItem>> PrinterQueue()
        {
            return _printerService.ListPrintQueue();
        }

        [HttpPost]
        [ActionName(nameof(ClearPrintQueue))]
        public Task ClearPrintQueue()
        {
            return _printerService.ClearPrintQueue();
        }
    }
}
