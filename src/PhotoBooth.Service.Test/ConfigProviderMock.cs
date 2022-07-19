using Moq;
using PhotoBooth.Abstraction.Configuration;

namespace PhotoBooth.Service.Test
{
    public class ConfigProviderMock
    {
        private readonly Mock<Abstraction.Configuration.IConfigurationProvider> _mock;
        private int _getAccessCount;
        private int _saveAccessCount;

        public ConfigProviderMock()
        {
            _mock = new Mock<Abstraction.Configuration.IConfigurationProvider>();
            _mock.Setup(m => m.LoadConfiguration<ConfigurationEntry<string>>(It.IsAny<string>())).Returns((string key) => LoadEntry(key));
            _mock.Setup(m => m.SaveConfiguration(It.IsAny<string>(), It.IsAny<ConfigurationEntry<string>>())).Callback((string key, ConfigurationEntry<string> value) => SaveEntry(key, value));
        }

        private void SaveEntry(string key, ConfigurationEntry<string> value)
        {
            _saveAccessCount++;
        }


        public IConfigurationProvider Object
        {
            get
            {
                return _mock.Object;
            }
        }

        public int GetAccessCount
        {
            get
            {
                return _getAccessCount;
            }
            set
            {
                _getAccessCount = value;
            }
        }

        public int SaveAccessCount
        {
            get
            {
                return _saveAccessCount;
            }
            set
            {
                _saveAccessCount = value;
            }
        }

        private ConfigurationEntry<string> LoadEntry(string key)
        {
            _getAccessCount++;

            return new ConfigurationEntry<string>()
            {
                Value = "Value"
            };
        }
    }
}
