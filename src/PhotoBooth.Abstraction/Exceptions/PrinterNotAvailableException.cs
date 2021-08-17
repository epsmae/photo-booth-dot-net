namespace PhotoBooth.Abstraction.Exceptions
{
    public class PrinterNotAvailableException : PrinterException
    {
        public PrinterNotAvailableException(string message)
            : base(message)
        {
        }
    }
}
