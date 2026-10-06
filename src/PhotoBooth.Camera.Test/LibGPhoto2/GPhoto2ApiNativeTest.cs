using System;
using NUnit.Framework;
using PhotoBooth.Abstraction.LibGPhoto2;
using PhotoBooth.Camera.LibGPhoto2;

namespace PhotoBooth.Camera.Test.LibGPhoto2
{
    /// <summary>
    /// Calls the real libgphoto2 (skipped if the library is not installed: sudo apt-get install libgphoto2-6).
    /// Expects that no camera is connected.
    /// </summary>
    [Category("Native")]
    public class GPhoto2ApiNativeTest
    {
        private GPhoto2Api _api;

        [SetUp]
        public void Setup()
        {
            try
            {
                _api = new GPhoto2Api();
            }
            catch (DllNotFoundException ex)
            {
                Assert.Ignore($"libgphoto2 not installed: {ex.Message}");
            }
        }

        [TearDown]
        public void TearDown()
        {
            _api?.Dispose();
        }

        [Test]
        public void TestConnectWithoutCamera()
        {
            GPhoto2Exception exception = Assert.Throws<GPhoto2Exception>(() => _api.Connect());

            Assert.AreEqual(GPhoto2Exception.ErrorModelNotFound, exception.Code, exception.Message);
            Assert.True(exception.IsConnectionError);
            StringAssert.Contains("gp_camera_init", exception.Message);
            Assert.False(_api.IsConnected);
        }

        [Test]
        public void TestCallsRequireConnection()
        {
            Assert.Throws<GPhoto2Exception>(() => _api.CapturePreview());
            Assert.Throws<GPhoto2Exception>(() => _api.CaptureImage());

            // no-op without connection
            _api.Disconnect();
        }
    }
}
