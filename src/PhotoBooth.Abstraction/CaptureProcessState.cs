namespace PhotoBooth.Abstraction
{
    public enum CaptureProcessState
    {
        Initializing,
        Ready,
        CountDown,
        Capture,
        Error,
        Review,
        Print
    }
}
