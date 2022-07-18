using System;

namespace PhotoBooth.Abstraction.Configuration
{
    public class ConfigurationEntry
    {
        /// <summary>
        /// Unique id of the setting
        /// </summary>
        public string Id
        {
            get; set;
        }

        /// <summary>
        /// Last modification date
        /// </summary>
        public DateTime LastModified
        {
            get; set;
        }

        /// <summary>
        /// Full qualified type name
        /// </summary>
        public string FullSettingsTypeName
        {
            get; set;
        }
    }

    public class ConfigurationEntry<T> : ConfigurationEntry, IConfigurationInfo<T>
    {
        public ConfigurationEntry()
        {
            FullSettingsTypeName = typeof(T).AssemblyQualifiedName;
        }

        /// <summary>
        /// Default value of the configuration
        /// </summary>
        public T DefaultValue
        {
            get; set;
        }

        /// <summary>
        /// Value of the configuration
        /// </summary>
        public T Value
        {
            get; set;
        }
    }
}
