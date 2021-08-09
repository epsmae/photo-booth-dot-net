using System;
using System.Linq;
using PhotoBooth.Abstraction.Configuration;

namespace PhotoBooth.Service
{
    public class ConfigurationService: IConfigurationService
    {
        private const string StepDownDurationInSecondsKey = "step_down_duration";
        private const string CaptureCountDownStepCountKey = "capture_count_down_steps";
        private const string ReviewCountDownStepCountKey = "review_count_down_steps";

        private const double MinimalCountDownStepDurationMs = 0.2;
        private const int MinimalCountDownSteps = 3;
        private const int MinimalReviewCountDownSteps = 5;

        private readonly IConfigurationProvider _provider;

        public ConfigurationService(IConfigurationProvider provider)
        {
            _provider = provider;

            if (!_provider.LoadAvailableKeys().Any())
            {
                AddDefaultsValue();
            }
        }

        public double StepDownDurationInSeconds
        {
            get
            {
                return _provider.LoadEntry<double>(StepDownDurationInSecondsKey);
            }
            set
            {
                if (value < MinimalCountDownStepDurationMs)
                {
                    throw new ArgumentException($"Countdown steps duration has to be larger or equal as {MinimalCountDownStepDurationMs}");
                }

                _provider.AddOrUpdateEntry(StepDownDurationInSecondsKey, value);
            }
        }


        public int ReviewCountDownStepCount
        {
            get
            {
                return _provider.LoadEntry<int>(ReviewCountDownStepCountKey);
            }
            set
            {
                if (value < MinimalReviewCountDownSteps)
                {
                    throw new ArgumentException($"Review duration has to bo larger or equal as {MinimalReviewCountDownSteps}");
                }

                _provider.AddOrUpdateEntry(ReviewCountDownStepCountKey, value);
            }
        }

        public int CaptureCountDownStepCount
        {
            get
            {
                return _provider.LoadEntry<int>(CaptureCountDownStepCountKey);
            }
            set
            {
                if (value < MinimalCountDownSteps)
                {
                    throw new ArgumentException($"Countdown steps has to bo larger or equal as {MinimalCountDownSteps}");
                }

                _provider.AddOrUpdateEntry(CaptureCountDownStepCountKey, value);
            }
        }

        private void AddDefaultsValue()
        {
            StepDownDurationInSeconds = 1;
            ReviewCountDownStepCount = 10;
            CaptureCountDownStepCount = 3;
        }
    }
}
