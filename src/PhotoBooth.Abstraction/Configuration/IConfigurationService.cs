namespace PhotoBooth.Abstraction.Configuration
{
    public interface IConfigurationService
    {
        double StepDownDurationInSeconds { get; set; }
        int ReviewCountDownStepCount { get; set; }
        int CaptureCountDownStepCount { get; set; }
        int ReviewImageWidth { get; set; }
        string SelectedPrinter { get; set; }

        string SelectedCamera
        {
            get; set;
        }


        void Register<T>(string key, T defaultValue);
    }
}
