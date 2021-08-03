using System.IO;
using Moq;
using NUnit.Framework;
using PhotoBooth.Abstraction;

namespace PhotoBooth.Printer.Test
{
    public class PrinterAdapterMock
    {
        private readonly Mock<IPrinterAdapter> _mock;

        private bool _simulateFileError;
        private bool _simulatePrinterNotAvailable;
        private bool _simulateEmptyPrintQueue;
        private bool _simulateNoPrinterAvailable;
        private bool _simulateMultiplePrinterAvailable;

        private string SampleDirectory
        {
            get
            {
                return Path.Combine(TestContext.CurrentContext.TestDirectory, "SampleResponses");
            }
        }

        internal IPrinterAdapter Object
        {
            get
            {
                return _mock.Object;
            }
        }

        internal bool SimulateFileError
        {
            get
            {
                return _simulateFileError;
            }
            set
            {
                _simulateFileError = value;
            }
        }

        public bool SimulateEmptyPrintQueue
        {
            get
            {
                return _simulateEmptyPrintQueue;
            }
            set
            {
                _simulateEmptyPrintQueue = value;
            }
        }

        public bool SimulateNoPrinterAvailable
        {
            get
            {
                return _simulateNoPrinterAvailable;
            }
            set
            {
                _simulateNoPrinterAvailable = value;
            }
        }

        internal bool SimulatePrinterNotAvailable
        {
            get
            {
                return _simulatePrinterNotAvailable;
            }
            set
            {
                _simulatePrinterNotAvailable = value;
            }
        }

        internal bool SimulateMultiplePrinterAvailable
        {
            get
            {
                return _simulateMultiplePrinterAvailable;
            }
            set
            {
                _simulateMultiplePrinterAvailable = value;
            }
        }



        internal PrinterAdapterMock()
        {
            _mock = new Mock<IPrinterAdapter>();
            _mock.Setup(m => m.Print(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(
                (string printer, string file) => Print(printer, file));
            _mock.Setup(m => m.ClearPrintQueue()).ReturnsAsync(ClearPrintQueue);
            _mock.Setup(m => m.ListPrintQueue()).ReturnsAsync(ListPrintQueue);
            _mock.Setup(m => m.ListPrinters()).ReturnsAsync(ListPrinters);
        }

        private CommandLineResult ListPrinters()
        {
            if (SimulateNoPrinterAvailable)
            {
                return CreateSuccessResult("cups_available_printers_lpstats-e.txt");
            }

            if (SimulateMultiplePrinterAvailable)
            {
                return CreateSuccessResult("cups_available_printers_lpstats-e.txt");
            }

            return CreateSuccessResult("cups_available_printer_lpstats-e.txt");
        }

        private CommandLineResult ListPrintQueue()
        {
            if (SimulateEmptyPrintQueue)
            {
                return CreateSuccessResult("cups_printer_queue_empty_lpstat.txt");
            }

            return CreateSuccessResult("cups_printer_queue_lpstat.txt");
        }

        private CommandLineResult ClearPrintQueue()
        {
            return new CommandLineResult
            {
                StandardError = string.Empty,
                StandardOutput = string.Empty,
                ExitCode = 0
            };
        }


        private CommandLineResult Print(string printer, string file)
        {
            if (SimulateFileError)
            {
                return CreateSuccessResult("cups_file_or_printer_error.txt");
            }

            if (SimulatePrinterNotAvailable)
            {
                return CreateSuccessResult("cups_printer_not_available.txt");
            }

            return CreateSuccessResult("cups_print_request_success.txt");
        }


        private CommandLineResult CreateSuccessResult(string responseFile)
        {
            return new CommandLineResult
            {
                StandardError = string.Empty,
                StandardOutput = File.ReadAllText(Path.Combine(SampleDirectory, responseFile)),
                ExitCode = 0
            };
        }

        private CommandLineResult CreateErrorResult(string responseFile)
        {
            return new CommandLineResult
            {
                StandardError = File.ReadAllText(Path.Combine(SampleDirectory, responseFile)),
                StandardOutput = string.Empty,
                ExitCode = 1
            };
        }
    }
}
