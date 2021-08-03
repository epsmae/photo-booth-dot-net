using System.Threading.Tasks;
using NUnit.Framework;

namespace PhotoBooth.Console.Test
{
    public class PrintTest
    {
        [Test]
        public async Task TestPrint()
        {
            int result = await Program.Main(new[] { "print", "--printer", "MyPrinter", "--file", "MyFile" });
            Assert.AreEqual(ResultCodes.Success, result);
        }

        [Test]
        public async Task TestListPrinters()
        {
            int result = await Program.Main(new[] { "listPrinter" });
            Assert.AreEqual(ResultCodes.Success, result);
        }

        [Test]
        public async Task TestClearQueue()
        {
            int result = await Program.Main(new[] { "clearQueue" });
            Assert.AreEqual(ResultCodes.Success, result);
        }

        [Test]
        public async Task TestListQueue()
        {
            int result = await Program.Main(new[] { "listQueue" });
            Assert.AreEqual(ResultCodes.Success, result);
        }
    }
}
