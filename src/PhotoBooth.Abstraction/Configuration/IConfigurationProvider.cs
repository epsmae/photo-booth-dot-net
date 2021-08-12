using System.Collections.Generic;

namespace PhotoBooth.Abstraction.Configuration
{
    public interface IConfigurationProvider
    {
        void AddOrUpdateEntry<T>(string key, T value);

        T LoadEntry<T>(string key);

        List<string> LoadAvailableKeys();
    }
}
