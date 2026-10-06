using System.Threading.Tasks;
using NUnit.Framework;

namespace PhotoBooth.Console.Test
{
    public class LiveViewTest
    {
        [Test]
        public async Task TestLiveView()
        {
            int result = await Program.Main(new[] { "liveview", "--seconds", "2" });
            Assert.AreEqual(ResultCodes.Success, result);
        }

        [Test]
        public async Task TestLiveViewWithCapture()
        {
            int result = await Program.Main(new[] { "liveview", "--seconds", "1", "--capture" });
            Assert.AreEqual(ResultCodes.Success, result);
        }

        [Test]
        public async Task TestLibGPhoto2LiveViewSimulated()
        {
            int result = await Program.Main(new[] { "liveview-libgphoto2", "--seconds", "2", "--captures", "2", "--simulate" });
            Assert.AreEqual(ResultCodes.Success, result);
        }
    }
}
