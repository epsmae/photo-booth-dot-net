using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Newtonsoft.Json;
using PhotoBooth.Abstraction;
using PhotoBooth.Abstraction.Configuration;

namespace PhotoBooth.Service
{
    public class JsonConfigurationProviderProvider : IConfigurationProvider
    {
        private static readonly object Lock = new object();
        private readonly string _path;


        public JsonConfigurationProviderProvider(IFilePathProvider filePathProvider)
        {
            _path = Path.Combine(filePathProvider.ExecutionDirectory, "config.json");

            if (!File.Exists(_path))
            {
                Save(new List<ConfigurationEntry>());
            }
        }

        /// <summary>
        /// Register a configuration with a default value.
        /// If the entry is already existing nothing happens.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        public void RegisterEntry<T>(string key, T defaultValue)
        {
            lock (Lock)
            {
                List<ConfigurationEntry> entries = LoadEntries();
                ConfigurationEntry entry = entries.FirstOrDefault(e => e.Key == key);

                if (entry == null)
                {
                    string value = JsonConvert.SerializeObject(defaultValue);

                    entries.Add(new ConfigurationEntry
                    {
                        Type = defaultValue.GetType(),
                        Key = key,
                        Value = value
                    });

                    Save(entries);
                }
            }
        }

        /// <summary>
        /// Adds or updates a configuration entry
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddOrUpdateEntry<T>(string key, T value)
        {
            lock (Lock)
            {
                List<ConfigurationEntry> entries = LoadEntries();
                ConfigurationEntry entry = entries.FirstOrDefault(e => e.Key == key);

                if (entry == null)
                {
                    entries.Add(new ConfigurationEntry
                    {
                        Type = value.GetType(),
                        Key = key,
                        Value = JsonConvert.SerializeObject(value)
                    });
                }
                else
                {
                    entry.Type = value.GetType(),
                    entry.Value = JsonConvert.SerializeObject(value);
                }

                Save(entries);
            }
        }

        /// <summary>
        /// Load an entry.
        /// Throws exception when the key is not available
        /// or the type is different.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T LoadEntry<T>(string key)
        {
            lock (Lock)
            {
                List<ConfigurationEntry> entries = LoadEntries();
                ConfigurationEntry entry = entries.First(e => e.Key == key);

                return JsonConvert.DeserializeObject<T>(entry.Value);
            }
        }


        private void Save(List<ConfigurationEntry> entries)
        {
            lock (Lock)
            {
                string serializedObject = JsonConvert.SerializeObject(entries);
                File.WriteAllText(_path, serializedObject);
            }
        }

        /// <summary>
        /// Loads all saved keys.
        /// </summary>
        /// <returns></returns>
        public List<string> LoadAvailableKeys()
        {
            lock (Lock)
            {
                List<string> items = new List<string>();
                foreach (ConfigurationEntry entry in LoadEntries())
                {
                    items.Add(entry.Key);
                }

                return items;
            }
        }


        private List<ConfigurationEntry> LoadEntries()
        {
            lock (Lock)
            {
                string serializedObject = File.ReadAllText(_path);

                return JsonConvert.DeserializeObject<List<ConfigurationEntry>>(serializedObject);
            }
        }
    }
}
