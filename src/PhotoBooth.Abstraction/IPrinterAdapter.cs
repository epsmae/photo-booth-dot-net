using System.Threading.Tasks;

namespace PhotoBooth.Abstraction
{
    public interface IPrinterAdapter
    {
        Task<CommandLineResult> Print(string printerName, string fileName);

        Task<CommandLineResult> ListPrinters();

        Task<CommandLineResult> ListPrintQueue();

        Task<CommandLineResult> ClearPrintQueue();
    }
}