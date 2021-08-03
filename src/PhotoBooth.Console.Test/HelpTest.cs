using System.Threading.Tasks;
using NUnit.Framework;

namespace PhotoBooth.Console.Test
{
    public class HelpTest
    {
        [Test]
        public async Task TestVersion()
        {
            int result = await Program.Main(new[] { "--version" });
            Assert.AreEqual(ResultCodes.Success, result);
        }

        [Test]
        public async Task TestHelp()
        {
            int result = await Program.Main(new[] { "print", "--help" });
            Assert.AreEqual(ResultCodes.Success, result);
        }
    }
}