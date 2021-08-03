using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PhotoBooth.Abstraction;


namespace PhotoBooth.Console
{
    public class PrintCommandHandler
    {
        private readonly ILogger<PrintCommandHandler> _logger;
        private readonly IPrinterService _service;

        public PrintCommandHandler(ILogger<PrintCommandHandler> logger, IPrinterService service)
        {
            _logger = logger;
            _service = service;
        }
        
        public IList<Command> BuildPrintCommand()
        {
            IList<Command> items = new List<Command>();

            Command printCommand = new Command("print")
            {
                new Option<string>("--printer") {IsRequired = true, Description = "Printer name"},
                new Option<string>("--file") {IsRequired = true, Description = "File to print"}
            };

            printCommand.Handler = CommandHandler.Create(async (string printer, string file) => await Print(printer, file));
            items.Add(printCommand);

            Command listPrinter = new Command("listPrinter");
            listPrinter.Handler = CommandHandler.Create(async () => await ListPrinter());
            items.Add(listPrinter);

            Command clearQueue = new Command("clearQueue");
            clearQueue.Handler = CommandHandler.Create(async() => await ClearQueue());
            items.Add(clearQueue);

            Command listQueue = new Command("listQueue");
            listQueue.Handler = CommandHandler.Create(async () => await ListQueue());
            items.Add(listQueue);

            return items;
        }

        private async Task<int> ListQueue()
        {
            try
            {
                List<PrintQueueItem> items = await _service.ListPrintQueue();
                foreach (PrintQueueItem item in items)
                {
                    _logger.LogInformation($"{item.Name}");
                }
                return ResultCodes.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list print queue");
                return ResultCodes.Error;
            }
        }

        private async Task<int> ClearQueue()
        {
            try
            {
                await _service.ClearPrintQueue();
                return ResultCodes.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear queue");
                return ResultCodes.Error;
            }
        }

        private async Task<int> ListPrinter()
        {
            try
            {
                List<Abstraction.Printer> printers = await _service.ListPrinters();
                foreach (Abstraction.Printer printer in printers)
                {
                    _logger.LogInformation($"{printer}");   
                }
                return ResultCodes.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list printers");
                return ResultCodes.Error;
            }
        }

        private async Task<int> Print(string printer, string file)
        {
            try
            {
                await _service.Print(printer, file);
                return ResultCodes.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to print");
                return ResultCodes.Error;
            }
        }
    }
}
