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
                    if (entry.Type != value.GetType())
                    {
                        throw new Exception("Invalid type");
                    }

                    entry.Value = JsonConvert.SerializeObject(value);
                }

                Save(entries);
            }
        }

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
