using System;

namespace PhotoBooth.Abstraction.Exceptions
{
    public class PrinterException : Exception
    {
        public PrinterException(string message)
            :base(message)
        {

        }
    }
}
