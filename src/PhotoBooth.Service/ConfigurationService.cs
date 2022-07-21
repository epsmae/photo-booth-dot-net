using System;
using System.Collections.Generic;
using PhotoBooth.Abstraction.Configuration;

namespace PhotoBooth.Service
{
    public class ConfigurationService: IConfigurationService
    {
        private const double MinimalCountDownStepDurationMs = 0.2;
        private const int MinimalCountDownSteps = 3;
        private const int MinimalReviewCountDownSteps = 5;

        private static readonly object _updateLock = new object();

        private readonly IConfigurationProvider _provider;
        private readonly Dictionary<string, object> _cache;
        private readonly List<string> _keys;

        public ConfigurationService(IConfigurationProvider settingsProvider)
        {
            _provider = settingsProvider;
            _keys = new List<string>();
            _cache = new Dictionary<string, object>();
        }

        public int ReviewImageWidth
        {
            get
            {
                return GetValue<int>(ConfigurationKeys.ReviewImageWidth);
            }
            set
            {
                SetValue<int>(ConfigurationKeys.ReviewImageWidth, value);
            }
        }

        public int ReviewImageQuality
        {
            get
            {
                return GetValue<int>(ConfigurationKeys.ReviewImageQuality);
            }
            set
            {
                SetValue(ConfigurationKeys.ReviewImageQuality, value);
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

        public bool BlinkingEnabled
        {
            get
            {
                return GetValue<bool>(ConfigurationKeys.BlinkingEnabled);
            }
            set
            {
                SetValue(ConfigurationKeys.BlinkingEnabled, value);
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

        public IList<string> GetAllConfigurationIds()
        {
            IList<string> ids = new List<string>();

            lock (_updateLock)
            {
                foreach (string key in _keys)
                {
                    ids.Add(key);
                }
            }

            return ids;
        }
        
        public void Register<T>(string configurationId, T defaultValue)
        {
            lock (_updateLock)
            {
                if (!TryGetValue(configurationId, out ConfigurationEntry<T> entry))
                {
                    ConfigurationEntry<T> registerEntry = new ConfigurationEntry<T>
                    {
                        Id = configurationId,
                        DefaultValue = defaultValue,
                        Value = defaultValue,
                        LastModified = DateTime.MinValue,
                    };

                    _provider.SaveConfiguration(configurationId, registerEntry);
                    UpdateCache(configurationId, registerEntry.Value);
                }
                else
                {
                    entry.DefaultValue = defaultValue;
                    _provider.SaveConfiguration(configurationId, entry);
                    UpdateCache(configurationId, entry.Value);
                }

                if (!_keys.Contains(configurationId))
                {
                    _keys.Add(configurationId);
                }
            }
        }


        public T GetValue<T>(string settingsId)
        {
            lock (_updateLock)
            {
                if (_cache.ContainsKey(settingsId))
                {
                    return (T) _cache[settingsId];
                }

                ConfigurationEntry<T> entry = LoadConfiguration<T>(settingsId);

                UpdateCache(settingsId, entry.Value);

                return entry.Value;
            }
        }


        public void SetValue<T>(string configurationId, T value)
        {
            lock (_updateLock)
            {
                ConfigurationEntry<T> entry = LoadConfiguration<T>(configurationId);

                entry.Value = value;
                entry.LastModified = DateTime.Now;

                _provider.SaveConfiguration(configurationId, entry);
                UpdateCache(configurationId, entry.Value);
            }
        }
        
        private ConfigurationEntry<T> LoadConfiguration<T>(string configurationId)
        {
            return _provider.LoadConfiguration<ConfigurationEntry<T>>(configurationId);
        }

        private bool TryGetValue<T>(string configurationId, out ConfigurationEntry<T> value)
        {
            lock (_updateLock)
            {
                try
                {
                    value = _provider.LoadConfiguration<ConfigurationEntry<T>>(configurationId);
                    if (value != null)
                    {
                        return true;
                    }
                }
                catch (Exception)
                {
                    // ignore
                }

                value = null;
                return false;
            }
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
