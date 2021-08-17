namespace PhotoBooth.Abstraction.Exceptions
{
    public class CameraOutOfFocusException : CameraException
    {
        public CameraOutOfFocusException(string message)
            : base(message)
        {
        }
    }
}
