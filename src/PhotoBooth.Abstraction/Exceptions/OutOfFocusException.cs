namespace PhotoBooth.Abstraction.Exceptions
{
    public class OutOfFocusException : CameraException
    {
        public OutOfFocusException(string message)
            : base(message)
        {
        }
    }
}
