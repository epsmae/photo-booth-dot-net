using System;

namespace PhotoBooth.Abstraction.Exceptions
{
    public class CameraException : Exception
    {
        public CameraException(string message)
            :base(message)
        {

        }
    }
}
