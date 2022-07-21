using Moq;
using PhotoBooth.Abstraction.Configuration;

namespace PhotoBooth.Service.Test
{
    public class ConfigurationServiceMock
    {
        public const double StepCountDownDurationS = 0.5;
        public const int CountDownStepCount = 3;
        public const int ReviewStepCount = 5;
        public const int ImageWidth = 1024;
        public const string Printer = "MyPrinter";
        public const string Camera = "MyCamera";


        private readonly Mock<IConfigurationService> _mock;

        public ConfigurationServiceMock()
        {
            _mock = new Mock<IConfigurationService>();
            _mock.SetupGet(m => m.CaptureCountDownStepCount).Returns(CountDownStepCount);
            _mock.SetupGet(m => m.ReviewCountDownStepCount).Returns(ReviewStepCount);
            _mock.SetupGet(m => m.StepDownDurationInSeconds).Returns(StepCountDownDurationS);
            _mock.SetupGet(m => m.ReviewImageWidth).Returns(ImageWidth);
            _mock.SetupGet(m => m.SelectedPrinter).Returns(Printer);
            _mock.SetupGet(m => m.SelectedCamera).Returns(Camera);
        }

        public IConfigurationService Object
        {
            get
            {
                return _mock.Object;
            }
        }
    }
}
