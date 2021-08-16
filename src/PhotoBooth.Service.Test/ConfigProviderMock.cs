using Moq;

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
            _mock.Setup(m => m.LoadEntry<string>(It.IsAny<string>())).Returns((string key) => LoadEntry(key));
            _mock.Setup(m => m.AddOrUpdateEntry<string>(It.IsAny<string>(), It.IsAny<string>())).Callback((string key, string value) => SaveEntry(key, value));
        }

        private void SaveEntry(string key, string value)
        {
            _saveAccessCount++;
        }


        public Abstraction.Configuration.IConfigurationProvider Object
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

        private string LoadEntry(string key)
        {
            _getAccessCount++;

            return "Value";
        }
    }
}
