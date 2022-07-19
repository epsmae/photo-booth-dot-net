using NUnit.Framework;

namespace PhotoBooth.Service.Test
{
    public class ConfigurationServiceTest
    {
        private ConfigProviderMock _mock;
        private ConfigurationService _service;

        [SetUp]
        public void Setup()
        {
            _mock = new ConfigProviderMock();
            _service = new ConfigurationService(_mock.Object);
        }

        [Test]
        public void TestCache()
        {
            const string expectedValue = "New_Value";

            _service.SelectedCamera = "initial value";
            
            Assert.AreEqual(1, _mock.SaveAccessCount);
            Assert.AreEqual(0, _mock.GetAccessCount);

            string value1 = _service.SelectedCamera;

            Assert.AreEqual(1, _mock.SaveAccessCount);
            Assert.AreEqual(0, _mock.GetAccessCount);

            string value2 = _service.SelectedCamera;
            Assert.AreEqual(1, _mock.SaveAccessCount);
            Assert.AreEqual(0, _mock.GetAccessCount);


            _service.SelectedCamera = expectedValue;
            Assert.AreEqual(2, _mock.SaveAccessCount);
            Assert.AreEqual(1, _mock.GetAccessCount);

            string value3 = _service.SelectedCamera;
            Assert.AreEqual(expectedValue, value3);
            Assert.AreEqual(2, _mock.SaveAccessCount);
            Assert.AreEqual(1, _mock.GetAccessCount);

        }
    }
}
