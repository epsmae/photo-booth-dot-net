namespace PhotoBooth.Abstraction
{
    public class SettingsDto
    {
        public double StepDownDurationInSeconds
        {
            get;
            set;
        }

        public int ReviewCountDownStepCount
        {
            get;
            set;
        }

        public int CaptureCountDownStepCount
        {
            get;
            set;
        }

        public int ReviewImageWidth
        {
            get;
            set;
        }

        public int ReviewImageQuality
        {
            get;
            set;
        }

        public string SelectedCamera
        {
            get;
            set;
        }

        public string SelectedPrinter
        {
            get;
            set;
        }
    }
}
