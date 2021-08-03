using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoBooth.Abstraction
{
    public interface IPrinterService
    {
        Task Print(string printerName, string fileName);

        Task<List<Printer>> ListPrinters();
        Task<List<PrintQueueItem>> ListPrintQueue();

        Task ClearPrintQueue();
    }
}
