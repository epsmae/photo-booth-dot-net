using System;
using System.Threading.Tasks;
using Moq;
using PhotoBooth.Abstraction;

namespace PhotoBooth.Service.Test
{
    internal class PrinterServiceMock
    {
        private readonly Mock<IPrinterService> _mock;
        private bool _throwCaptureException;

        internal PrinterServiceMock()
        {
            _mock = new Mock<IPrinterService>();
            _mock.Setup(m => m.Print(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string printer, string fileName) => Printer(printer, fileName));
        }

        private async Task Printer(string printer, string fileName)
        {
            if (_throwCaptureException)
            {
                throw new Exception("Mock Camera Exception");
            }

            await Task.Delay(200);
        }

        internal IPrinterService Object
        {
            get
            {
                return _mock.Object;
            }
        }

        public void ThrowPrinterException()
        {
            _throwCaptureException = true;
        }
    }
}
