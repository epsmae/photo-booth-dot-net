using System.Collections.Generic;

namespace PhotoBooth.Service
{
    public class Configuration
    {
        public Configuration()
        {
            Items = new Dictionary<string, string>();
        }

        public Dictionary<string, string> Items
        {
            get; set;
        }
    }
}
