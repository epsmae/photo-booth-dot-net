using Moq;
using PhotoBooth.Abstraction.Configuration;

namespace PhotoBooth.Service.Test
{
    public class ConfigurationServiceMock
    {
        public const double StepCountDownDurationMs = 0.05;
        public const int CountDownStepCount = 3;
        public const int ReviewStepCount = 5;


        private readonly Mock<IConfigurationService> _mock;

        public ConfigurationServiceMock()
        {
            _mock = new Mock<IConfigurationService>();
            _mock.SetupGet(m => m.CaptureCountDownStepCount).Returns(CountDownStepCount);
            _mock.SetupGet(m => m.ReviewCountDownStepCount).Returns(ReviewStepCount);
            _mock.SetupGet(m => m.StepDownDurationInSeconds).Returns(StepCountDownDurationMs);
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
