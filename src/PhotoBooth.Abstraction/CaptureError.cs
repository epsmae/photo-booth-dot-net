using PhotoBooth.Abstraction.Exceptions;

namespace PhotoBooth.Abstraction
{
    public class CaptureError
    {
        public string ErrorMessage
        {
            get;
            set;
        }

        public PhotoBoothExceptions Exception
        {
            get;
            set;
        }
    }
}
