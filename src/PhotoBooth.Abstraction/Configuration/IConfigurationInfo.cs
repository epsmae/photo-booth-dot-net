using System;

namespace PhotoBooth.Abstraction.Configuration
{
    public interface IConfigurationInfo<T>
    {
        /// <summary>
        /// Unique id of the setting
        /// </summary>
        string Id
        {
            get;
        }

        /// <summary>
        /// Last modification date
        /// </summary>
        DateTime LastModified
        {
            get;
        }

        /// <summary>
        /// Default value of the configuration
        /// </summary>
        T DefaultValue
        {
            get;
        }

        /// <summary>
        /// Value of the configuration
        /// </summary>
        T Value
        {
            get;
        }
    }
}
