namespace PhotoBooth.Abstraction.Exceptions
{
    public class PtpStoreException : CameraException
    {
        public PtpStoreException()
            : base("Ptp Store exception, missing an sd card?")
        {
        }
    }
}
