namespace PhotoBooth.Service
{
    public enum CaptureTriggers
    {
        ConfirmError,
        InitializationDone,
        Capture,
        CountdownElapsed,
        CaptureCompleted,
        IntermediateCaptureCompleted,
        ReviewCountDownElapsed,
        Skip,
        Print,
        PrintCompleted,
        Error
    }
}
