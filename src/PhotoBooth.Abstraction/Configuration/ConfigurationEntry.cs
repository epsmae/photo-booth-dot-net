using System;

namespace PhotoBooth.Abstraction.Configuration
{
    public class ConfigurationEntry
    {
        public string Key
        {
            get;
            set;
        }

        public string Value
        {
            get;
            set;
        }

        public Type Type
        {
            get;
            set;
        }
    }
}
