namespace PhotoBooth.Service
{
    public enum CaptureTriggers
    {
        ConfirmError,
        InitializationDone,
        Capture,
        CountdownElapsed,
        CaptureCompleted,
        ReviewCountDownElapsed,
        Skip,
        Print,
        PrintCompleted,
        Error
    }
}
