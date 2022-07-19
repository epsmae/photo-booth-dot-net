using System;
using System.IO;
using NUnit.Framework;

namespace PhotoBooth.Service.Test
{
    public class ConfigurationProviderTest
    {
        private ConfigurationService _service;

        public string ConfigDirectory
        {
            get
            {
                return Path.Combine(TestContext.CurrentContext.TestDirectory, "Configurations");
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

            FilePathProviderMock mock = new FilePathProviderMock();
            mock.ExecutionDirectory = ConfigDirectory;
            JsonConfigurationProvider provider = new JsonConfigurationProvider(mock.Object);
            _service = new ConfigurationService(provider);
        }

        [Test]
        public void TestRegisterEntryNotExisting()
        {
            Assert.AreEqual(0, _service.GetAllConfigurationIds().Count);

            string userNameDefault = "MyDefaultUserName";
            string userNameDefaultKey = "user_name_default";
            _service.Register(userNameDefaultKey, userNameDefault);
            
            Assert.AreEqual(1, _service.GetAllConfigurationIds().Count);

            Assert.AreEqual(userNameDefault, _service.GetValue<string>(userNameDefaultKey));
        }

        [Test]
        public void TestRegisterEntryExisting()
        {
            Assert.AreEqual(0, _service.GetAllConfigurationIds().Count);

            string userName = "MyUserName";
            string userNameKey = "user_name";
            _service.SetValue(userNameKey, userName);
            Assert.AreEqual(userName, _service.GetValue<string>(userNameKey));

            string userNameDefault = "MyDefaultUserName";
            _service.Register(userNameKey, userNameDefault);

            Assert.AreEqual(userName, _service.GetValue<string>(userNameKey));
        }


        [Test]
        public void TestSaveLoadLoadMultiple()
        {
            Assert.AreEqual(0, _service.GetAllConfigurationIds().Count );

            string userName = "MyUserName";
            string userNameKey = "user_name";
            _service.SetValue(userNameKey, userName);

            int userAge = 25;
            string userAgeKey = "user_age";
            _service.SetValue(userAgeKey, userAge);

            Assert.AreEqual(2, _service.GetAllConfigurationIds().Count);

            Assert.AreEqual(userName, _service.GetValue<string>(userNameKey));
            Assert.AreEqual(userAge, _service.GetValue<int>(userAgeKey));
        }

        [Test]
        public void TestUpdate()
        {
            Assert.AreEqual(0, _service.GetAllConfigurationIds().Count);

            string userName = "MyUserName";
            string userNameKey = "user_name";
            _service.SetValue(userNameKey, userName);

            Assert.AreEqual(1, _service.GetAllConfigurationIds().Count);
            Assert.AreEqual(userName, _service.GetValue<string>(userNameKey));

            string updatedName = "MyNewUserName";
            _service.SetValue(userNameKey, updatedName);

            Assert.AreEqual(1, _service.GetAllConfigurationIds().Count);
            Assert.AreEqual(updatedName, _service.GetValue<string>(userNameKey));
        }

        [Test]
        public void TestSaveLoadLoadDouble()
        {
            Assert.AreEqual(0, _service.GetAllConfigurationIds().Count);

            double exposure = 1.8;
            string exposureKey = "exposure";
            _service.SetValue(exposureKey, exposure);

            Assert.AreEqual(1, _service.GetAllConfigurationIds().Count);

            Assert.AreEqual(exposure, _service.GetValue<double>(exposureKey), 0.001);
        }

        [Test]
        public void TestSaveLoadObject()
        {
            Assert.AreEqual(0, _service.GetAllConfigurationIds().Count);

            TestObject testObject = new TestObject {Age = 19, Name = "John"};
            string testObjectKey = "test_object";
            _service.SetValue(testObjectKey, testObject);

            Assert.AreEqual(1, _service.GetAllConfigurationIds().Count);

            TestObject loadedObject = _service.GetValue<TestObject>(testObjectKey);

            Assert.AreEqual(testObject.Age, loadedObject.Age);
            Assert.AreEqual(testObject.Name, loadedObject.Name);
        }
    }
}
