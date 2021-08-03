using System.Threading.Tasks;
using NUnit.Framework;

namespace PhotoBooth.Console.Test
{
    public class CameraTest
    {
        [Test]
        public async Task TestCapture()
        {
            int result = await Program.Main(new[] { "capture" });
            Assert.AreEqual(ResultCodes.Success, result);
        }

        [Test]
        public async Task TestListCameras()
        {
            int result = await Program.Main(new[] { "listCameras" });
            Assert.AreEqual(ResultCodes.Success, result);
        }

        [Test]
        public async Task TestCameraStatus()
        {
            int result = await Program.Main(new[] { "cameraStatus" });
            Assert.AreEqual(ResultCodes.Success, result);
        }
    }
}
