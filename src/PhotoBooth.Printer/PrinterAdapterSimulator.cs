using System.Threading.Tasks;
using PhotoBooth.Abstraction;

namespace PhotoBooth.Printer
{
    public class PrinterAdapterSimulator : IPrinterAdapter
    {
        public async Task<CommandLineResult> Print(string printerName, string fileName)
        {
            await Task.Delay(5000);
            return CreateSuccessResult();
        }

        public async Task<CommandLineResult> ListPrinters()
        {
            await Task.Delay(5000);
            return CreateSuccessResult();
        }

        public async Task<CommandLineResult> ListPrintQueue()
        {
            await Task.Delay(5000);
            return CreateSuccessResult();
        }

        public async Task<CommandLineResult> ClearPrintQueue()
        {
            await Task.Delay(5000);
            return CreateSuccessResult();
        }

        private static CommandLineResult CreateSuccessResult()
        {
            return new CommandLineResult
            {
                StandardError = string.Empty,
                StandardOutput = string.Empty,
                ExitCode = 0
            };
        }
    }
}
