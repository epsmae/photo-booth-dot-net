using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PhotoBooth.Abstraction;
using PhotoBooth.Abstraction.Exceptions;

namespace PhotoBooth.Printer
{
    public class PrinterService : IPrinterService
    {
        private readonly ILogger<PrinterService> _logger;
        private readonly IPrinterAdapter _adapter;

        public PrinterService(ILogger<PrinterService> logger, IPrinterAdapter adapter)
        {
            _logger = logger;
            _adapter = adapter;
        }
        
        public async Task Print(string printerName, string fileName)
        {
            await _adapter.EnablePrinter(printerName);
            CommandLineResult result = await _adapter.Print(printerName, fileName);
            LogResult(result);
            EvaluateResult(result);
            
            if (result.StandardOutput.ToLower().Contains("error"))
            {
                throw new PrinterException($"Failed to print: {result.StandardOutput}");
            }

            if (result.StandardOutput.ToLower().Contains("no such file or directory"))
            {
                throw new PrinterException($"Failed to print: {result.StandardOutput}");
            }
        }


        public async Task<List<Abstraction.Printer>> ListPrinters()
        {
            CommandLineResult result = await _adapter.ListPrinters();
            LogResult(result);
            EvaluateResult(result);

            List<Abstraction.Printer> printers = new List<Abstraction.Printer>();

            string[] lines = result.StandardOutput.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                printers.Add(new Abstraction.Printer() { Name = line.Trim() });
            }

            return printers;
        }


        public async Task<List<PrintQueueItem>> ListPrintQueue()
        {
            CommandLineResult result = await _adapter.ListPrintQueue();
            LogResult(result);
            EvaluateResult(result);

            List<PrintQueueItem> items = new List<PrintQueueItem>();
            
            string[] lines = result.StandardOutput.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                string[] entries = line.Split("  ", StringSplitOptions.RemoveEmptyEntries);

                if (entries.Length > 0)
                {
                    items.Add(new PrintQueueItem() { Name = entries[0].Trim() });
                }
            }

            return items;
        }
        
        public async Task ClearPrintQueue()
        {
            CommandLineResult result = await _adapter.ClearPrintQueue();
            LogResult(result);
            EvaluateResult(result);
        }


        private void LogResult(CommandLineResult result)
        {
            _logger.LogInformation($"ExitCode: {result.ExitCode}");
            _logger.LogInformation($"StandardOutput: {result.StandardOutput}");
            _logger.LogInformation($"StandardError: {result.StandardError}");
        }

        private void EvaluateResult(CommandLineResult result)
        {
            if (result.ExitCode != 0)
            {
                throw new PrinterException(result.StandardError);
            }
        }
    }
}
