using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using Microsoft.Extensions.Logging;
using PhotoBooth.Abstraction;

namespace PhotoBooth.Printer
{
    public class CupsPrinterAdapter : IPrinterAdapter
    {
        // https://www.cups.org/doc/man-lpstat.html
        // https://www.cups.org/doc/man-lp.html
        
        private readonly ILogger<CupsPrinterAdapter> _logger;

        public CupsPrinterAdapter(ILogger<CupsPrinterAdapter> logger)
        {
            _logger = logger;
        }
        
        public async Task<CommandLineResult> Print(string printerName, string fileName)
        {
            _logger.LogInformation($"Printer={printerName}, file={fileName}");

            BufferedCommandResult result = await Cli.Wrap("lp")
                .WithArguments($"-d {printerName} {fileName}")
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();

            return MapResult(result);
        }

        public async Task<CommandLineResult> ListPrinters()
        {
            _logger.LogInformation($"List all printers");

            BufferedCommandResult result = await Cli.Wrap("lpstat")
                .WithArguments($"-e")
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();

            return MapResult(result);
        }

        public async Task<CommandLineResult> ListPrintQueue()
        {
            _logger.LogInformation($"List print queue");

            BufferedCommandResult result = await Cli.Wrap("lpstat")
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();

            return MapResult(result);
        }
        
        public async Task<CommandLineResult> ClearPrintQueue()
        {
            _logger.LogInformation($"Cancel all jobs");

            BufferedCommandResult result = await Cli.Wrap("cancel")
                .WithArguments($"-a")
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();

            return MapResult(result);
        }

        private static CommandLineResult MapResult(BufferedCommandResult result)
        {
            return new CommandLineResult
            {
                ExitCode = result.ExitCode,
                StandardError = result.StandardError,
                StandardOutput = result.StandardOutput
            };
        }
    }
}
