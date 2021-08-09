using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions;
using NUnit.Framework;
using PhotoBooth.Abstraction.Configuration;

namespace PhotoBooth.Service.Test
{
    public class ConfigurationProviderTest
    {
        private JsonConfigurationProviderProvider _provider;

        public string ConfigDirectory
        {
            get
            {
                return Path.Combine(TestContext.CurrentContext.TestDirectory, "Configurations");
            }
        }

        public string ConfigFile
        {
            get
            {
                return Path.Combine(ConfigDirectory, "config.json");
            }
        }

        [SetUp]
        public void Setup()
        {

            if (Directory.Exists(ConfigDirectory))
            {
                Directory.Delete(ConfigDirectory, true);
            }

            Directory.CreateDirectory(ConfigDirectory);

            _provider = new JsonConfigurationProviderProvider(ConfigFile);
        }

        [Test]
        public void TestSaveLoadLoadMultiple()
        {
            Assert.AreEqual(0, _provider.LoadAvailableKeys().Count );

            string userName = "MyUserName";
            string userNameKey = "user_name";
            _provider.AddOrUpdateEntry(userNameKey, userName);

            int userAge = 25;
            string userAgeKey = "user_age";
            _provider.AddOrUpdateEntry(userAgeKey, userAge);

            Assert.AreEqual(2, _provider.LoadAvailableKeys().Count);

            Assert.AreEqual(userName, _provider.LoadEntry<string>(userNameKey));
            Assert.AreEqual(userAge, _provider.LoadEntry<int>(userAgeKey));
        }

        [Test]
        public void TestUpdate()
        {
            Assert.AreEqual(0, _provider.LoadAvailableKeys().Count);

            string userName = "MyUserName";
            string userNameKey = "user_name";
            _provider.AddOrUpdateEntry(userNameKey, userName);

            Assert.AreEqual(1, _provider.LoadAvailableKeys().Count);
            Assert.AreEqual(userName, _provider.LoadEntry<string>(userNameKey));

            string updatedName = "MyNewUserName";
            _provider.AddOrUpdateEntry(userNameKey, updatedName);

            Assert.AreEqual(1, _provider.LoadAvailableKeys().Count);
            Assert.AreEqual(updatedName, _provider.LoadEntry<string>(userNameKey));
        }

        [Test]
        public void TestSaveLoadLoadDouble()
        {
            Assert.AreEqual(0, _provider.LoadAvailableKeys().Count);

            double exposure = 1.8;
            string exposureKey = "exposure";
            _provider.AddOrUpdateEntry(exposureKey, exposure);

            Assert.AreEqual(1, _provider.LoadAvailableKeys().Count);

            Assert.AreEqual(exposure, _provider.LoadEntry<double>(exposureKey), 0.001);
        }

        [Test]
        public void TestSaveLoadObject()
        {
            Assert.AreEqual(0, _provider.LoadAvailableKeys().Count);

            TestObject testObject = new TestObject {Age = 19, Name = "John"};
            string testObjectKey = "test_object";
            _provider.AddOrUpdateEntry(testObjectKey, testObject);

            Assert.AreEqual(1, _provider.LoadAvailableKeys().Count);


            TestObject loadedObject = _provider.LoadEntry<TestObject>(testObjectKey);

            Assert.AreEqual(testObject.Age, loadedObject.Age);
            Assert.AreEqual(testObject.Name, loadedObject.Name);
        }
    }

}
