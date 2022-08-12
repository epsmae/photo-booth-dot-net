namespace PhotoBooth.Abstraction.Exceptions
{
    public class CameraFileNotFoundException : CameraException
    {
        public CameraFileNotFoundException(string message)
            : base(message)
        {
        }
    }
}
