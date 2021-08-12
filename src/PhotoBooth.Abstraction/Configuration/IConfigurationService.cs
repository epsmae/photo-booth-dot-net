namespace PhotoBooth.Abstraction.Configuration
{
    public interface IConfigurationService
    {
        double StepDownDurationInSeconds { get; set; }
        int ReviewCountDownStepCount { get; set; }
        int CaptureCountDownStepCount { get; set; }
    }
}
