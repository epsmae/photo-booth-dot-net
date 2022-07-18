using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using PhotoBooth.Abstraction;
using PhotoBooth.Abstraction.Configuration;

namespace PhotoBooth.Service
{
    public class JsonConfigurationProvider : IConfigurationProvider
    {
        private readonly string _configFile;
        private static readonly object FileLock = new object();

        public JsonConfigurationProvider(IFilePathProvider filePathProvider)
        {
            _configFile = Path.Combine(filePathProvider.ExecutionDirectory, "config.json");
        }
        
        public void DeleteAll()
        {
            Configuration config = LoadConfiguration();
            config.Items.Clear();
            TrySaveConfiguration(config);
        }

        public void SaveConfiguration<T>(string configurationId, ConfigurationEntry<T> value)
        {
            Configuration config = LoadConfiguration();

            if (!config.Items.ContainsKey(configurationId))
            {
                config.Items.Add(configurationId, JsonSerializer.Serialize(value));
            }
            else
            {
                config.Items[configurationId] = JsonSerializer.Serialize(value);
            }

            TrySaveConfiguration(config);
        }

        public T LoadConfiguration<T>(string configurationId) where T : ConfigurationEntry
        {
            Configuration config = LoadConfiguration();

            if (!config.Items.ContainsKey(configurationId))
            {
                throw new KeyNotFoundException($"Key={configurationId} not found");
            }

            return JsonSerializer.Deserialize<T>(config.Items[configurationId]);
        }

        public ConfigurationEntry LoadConfiguration(string configurationId)
        {
            return LoadConfiguration<ConfigurationEntry>(configurationId);
        }

        public void Delete(string configurationId)
        {
            Configuration config = LoadConfiguration();
            config.Items.Remove(configurationId);
            TrySaveConfiguration(config);
        }

        private void TrySaveConfiguration(Configuration configuration)
        {
            try
            {
                lock (FileLock)
                {
                    File.WriteAllText(_configFile, JsonSerializer.Serialize(configuration));
                }
            }
            catch
            {
                //ignore
            }
        }

        private Configuration LoadConfiguration()
        {
            try
            {
                if (File.Exists(_configFile))
                {
                    lock (FileLock)
                    {
                        return JsonSerializer.Deserialize<Configuration>(File.ReadAllText(_configFile));
                    }
                }
            }
            catch
            {
                // ignore
            }

            return new Configuration();
        }
    }
}