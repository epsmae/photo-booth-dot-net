namespace PhotoBooth.Abstraction.Exceptions
{
    public class NoCameraAvailableException : CameraException
    {
        public NoCameraAvailableException(string message)
            : base(message)
        {
        }
    }
}
