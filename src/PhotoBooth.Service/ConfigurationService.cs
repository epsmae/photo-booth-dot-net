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
        private readonly Dictionary<string, object> _cache;

        public ConfigurationService(IConfigurationProvider provider)
        {
            _provider = provider;
            _cache = new Dictionary<string, object>();
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
                return GetValue<string>(ConfigurationKeys.SelectedCamera);
            }
            set
            {
                SetValue(ConfigurationKeys.SelectedCamera, value);
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
                return GetValue<int>(ConfigurationKeys.ReviewImageWidth);
            }
            set
            {
                SetValue(ConfigurationKeys.ReviewImageWidth, value);
            }
        }

        public string SelectedPrinter
        {
            get
            {
                return GetValue<string>(ConfigurationKeys.SelectedPrinter);
            }
            set
            {
                SetValue(ConfigurationKeys.SelectedPrinter, value);
            }
        }

        public double StepDownDurationInSeconds
        {
            get
            {
                return GetValue<double>(ConfigurationKeys.StepDownDurationInSeconds);
            }
            set
            {
                if (value < MinimalCountDownStepDurationMs)
                {
                    throw new ArgumentException($"Countdown steps duration has to be larger or equal as {MinimalCountDownStepDurationMs}");
                }

                SetValue(ConfigurationKeys.StepDownDurationInSeconds, value);
            }
        }


        public int ReviewCountDownStepCount
        {
            get
            {
                return GetValue<int>(ConfigurationKeys.ReviewCountDownStepCount);
            }
            set
            {
                if (value < MinimalReviewCountDownSteps)
                {
                    throw new ArgumentException($"Review duration has to bo larger or equal as {MinimalReviewCountDownSteps}");
                }

                SetValue(ConfigurationKeys.ReviewCountDownStepCount, value);
            }
        }

        public int CaptureCountDownStepCount
        {
            get
            {
                return GetValue<int>(ConfigurationKeys.CaptureCountDownStepCount);
            }
            set
            {
                if (value < MinimalCountDownSteps)
                {
                    throw new ArgumentException($"Countdown steps has to bo larger or equal as {MinimalCountDownSteps}");
                }

                SetValue(ConfigurationKeys.CaptureCountDownStepCount, value);
            }
        }


        private T GetValue<T>(string key)
        {
            if (_cache.ContainsKey(key))
            {
                return (T) _cache[key];
            }

            T value = _provider.LoadEntry<T>(key);
            UpdateCache(key, value);

            return value;
        }

        private void SetValue<T>(string key, T value)
        {
            _provider.AddOrUpdateEntry(key, value);

            UpdateCache(key, value);
        }

        private void UpdateCache<T>(string key, T value)
        {
            if (_cache.ContainsKey(key))
            {
                _cache[key] = value;
            }
            else
            {
                _cache.Add(key, value);
            }
        }
    }
}
