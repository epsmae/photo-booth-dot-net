namespace PhotoBooth.Abstraction.Exceptions
{
    public class CameraNotAvailableException : CameraException
    {
        public CameraNotAvailableException(string message)
            : base(message)
        {
        }
    }
}
