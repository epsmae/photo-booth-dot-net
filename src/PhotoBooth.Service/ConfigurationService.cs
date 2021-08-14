using System;
using System.Collections.Generic;
using System.Linq;
using PhotoBooth.Abstraction.Configuration;

namespace PhotoBooth.Service
{
    public class ConfigurationService: IConfigurationService
    {
        private const double MinimalCountDownStepDurationMs = 0.2;
        private const int MinimalCountDownSteps = 3;
        private const int MinimalReviewCountDownSteps = 5;

        private readonly IConfigurationProvider _provider;

        public ConfigurationService(IConfigurationProvider provider)
        {
            _provider = provider;
        }

        public List<string> AvailableKeys
        {
            get
            {
                return _provider.LoadAvailableKeys();
            }
        }

        public string SelectedCamera
        {
            get
            {
                return _provider.LoadEntry<string>(ConfigurationKeys.SelectedCamera);
            }
            set
            {
                _provider.AddOrUpdateEntry(ConfigurationKeys.SelectedCamera, value);
            }
        }

        public void Register<T>(string key, T defaultValue)
        {
            _provider.RegisterEntry(key, defaultValue);
        }

        public int ReviewImageWidth
        {
            get
            {
                return _provider.LoadEntry<int>(ConfigurationKeys.ReviewImageWidth);
            }
            set
            {
                _provider.AddOrUpdateEntry(ConfigurationKeys.ReviewImageWidth, value);
            }
        }

        public string SelectedPrinter
        {
            get
            {
                return _provider.LoadEntry<string>(ConfigurationKeys.SelectedPrinter);
            }
            set
            {
                _provider.AddOrUpdateEntry(ConfigurationKeys.SelectedPrinter, value);
            }
        }

        public double StepDownDurationInSeconds
        {
            get
            {
                return _provider.LoadEntry<double>(ConfigurationKeys.StepDownDurationInSeconds);
            }
            set
            {
                if (value < MinimalCountDownStepDurationMs)
                {
                    throw new ArgumentException($"Countdown steps duration has to be larger or equal as {MinimalCountDownStepDurationMs}");
                }

                _provider.AddOrUpdateEntry(ConfigurationKeys.StepDownDurationInSeconds, value);
            }
        }


        public int ReviewCountDownStepCount
        {
            get
            {
                return _provider.LoadEntry<int>(ConfigurationKeys.ReviewCountDownStepCount);
            }
            set
            {
                if (value < MinimalReviewCountDownSteps)
                {
                    throw new ArgumentException($"Review duration has to bo larger or equal as {MinimalReviewCountDownSteps}");
                }

                _provider.AddOrUpdateEntry(ConfigurationKeys.ReviewCountDownStepCount, value);
            }
        }

        public int CaptureCountDownStepCount
        {
            get
            {
                return _provider.LoadEntry<int>(ConfigurationKeys.CaptureCountDownStepCount);
            }
            set
            {
                if (value < MinimalCountDownSteps)
                {
                    throw new ArgumentException($"Countdown steps has to bo larger or equal as {MinimalCountDownSteps}");
                }

                _provider.AddOrUpdateEntry(ConfigurationKeys.CaptureCountDownStepCount, value);
            }
        }
    }
}
