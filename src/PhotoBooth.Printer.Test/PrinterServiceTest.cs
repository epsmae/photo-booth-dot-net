using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using PhotoBooth.Abstraction;
using PhotoBooth.Abstraction.Exceptions;

namespace PhotoBooth.Printer.Test
{
    public class PrinterServiceTest
    {
        private PrinterAdapterMock _mock;
        private PrinterService _service;

        [SetUp]
        public void Setup()
        {
            _mock = new PrinterAdapterMock();
            _service = new PrinterService(NullLogger<PrinterService>.Instance, _mock.Object);
        }
        
        [Test]
        public async Task TestPrintSuccessful()
        {
            await _service.Print("MyPrinter", "Test.jpg");
        }
        
        [Test]
        public void TestPrintNotSuccessfulFileError()
        {
            _mock.SimulateFileError = true;
            
            Assert.CatchAsync<PrinterException>(async () => await _service.Print("MyPrinter", "Test.jpg"));
        }

        [Test]
        public void TestPrintNotSuccessfulPrinterError()
        {
            _mock.SimulatePrinterNotAvailable = true;

            Assert.CatchAsync<PrinterException>(async () => await _service.Print("MyPrinter", "Test.jpg"));
        }

        [Test]
        public async Task TestListPrinter()
        {
            List<Abstraction.Printer> printers = await _service.ListPrinters();
            Assert.AreEqual(1, printers.Count);
            Assert.AreEqual("Canon_SELPHY_CP1300", printers[0].Name);
        }


        [Test]
        public async Task TestListPrintQueue()
        {
            List<PrintQueueItem> items = await _service.ListPrintQueue();
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual("Canon_SELPHY_CP1300-3", items[0].Name);
            Assert.AreEqual("Canon_SELPHY_CP1300-4", items[1].Name);
        }

        [Test]
        public async Task TestListPrintQueueEmpty()
        {
            _mock.SimulateEmptyPrintQueue = true;
            List<PrintQueueItem> items = await _service.ListPrintQueue();
            Assert.AreEqual(0, items.Count);
        }

        [Test]
        public async Task TestClearPrintQueue()
        {
             await _service.ClearPrintQueue();
        }
    }
}