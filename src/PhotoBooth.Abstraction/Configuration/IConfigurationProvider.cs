using System.Collections.Generic;

namespace PhotoBooth.Abstraction.Configuration
{
    public interface IConfigurationProvider
    {
        /// <summary>
        /// Register a configuration with a default value.
        /// If the entry is already existing nothing happens.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        void RegisterEntry<T>(string key, T defaultValue);

        /// <summary>
        /// Adds or updates a configuration entry
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void AddOrUpdateEntry<T>(string key, T value);

        /// <summary>
        /// Load an entry.
        /// Throws exception when the key is not available
        /// or the type is different.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T LoadEntry<T>(string key);

        /// <summary>
        /// Loads all saved keys.
        /// </summary>
        /// <returns></returns>
        List<string> LoadAvailableKeys();
    }
}
