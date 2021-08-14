namespace PhotoBooth.Abstraction.Exceptions
{
    public class NoPrinterAvailableException : PrinterException
    {
        public NoPrinterAvailableException(string message)
            : base(message)
        {
        }
    }
}
