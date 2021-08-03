using System;

namespace PhotoBooth.Abstraction.Exceptions
{
    public class InvalidStateException : Exception
    {
        public InvalidStateException(string message)
            :base(message)
        {

        }
    }
}
