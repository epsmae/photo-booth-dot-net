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
            await Task.Delay(200);
            return CreateSuccessResult($"Canon_SELPHY_CP1300");
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

        public Task<CommandLineResult> EnablePrinter(string printerName)
        {
            return Task.FromResult(CreateSuccessResult());
        }

        private static CommandLineResult CreateSuccessResult()
        {
            return CreateSuccessResult(string.Empty);
        }

        private static CommandLineResult CreateSuccessResult(string output)
        {
            return new CommandLineResult
            {
                StandardError = string.Empty,
                StandardOutput = output,
                ExitCode = 0
            };
        }
    }
}
