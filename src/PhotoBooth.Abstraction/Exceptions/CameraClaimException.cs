namespace PhotoBooth.Abstraction.Exceptions
{
    public class CameraClaimException : CameraException
    {
        public CameraClaimException(string message)
            : base(message)
        {
        }
    }
}
