using System.IO;
using NUnit.Framework;
using PhotoBooth.Abstraction;

namespace PhotoBooth.Service.Test
{
    public class ImageResizerTest
    {
        private ImageResizer _resizer;

        private string SourceImagePath
        {
            get
            {
                return Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "SampleImage.jpg");
            }
        }

        private string DestinationImagePath
        {
            get
            {
                return Path.Combine(TestContext.CurrentContext.WorkDirectory, "SampleImageResized.jpg");
            }
        }


        [SetUp]
        public void Setup()
        {
            _resizer = new ImageResizer();
        }

        [Test]
        public void TestResize()
        {
            const int expectedWidth = 1024;
            const int expectedHeight = 678;

            using (Stream stream = File.OpenRead(SourceImagePath))
            {
                ImageDimensions srcDimension = _resizer.LoadImageInfo(stream);
                Assert.True(srcDimension.Width > expectedWidth);
                Assert.True(srcDimension.Height > expectedHeight);
            }
            
            using (Stream stream = File.OpenRead(SourceImagePath))
            {
                byte[] data = _resizer.ResizeImage(stream, expectedWidth, 50);
                File.WriteAllBytes(DestinationImagePath, data);
            }

            using (Stream stream = File.OpenRead(DestinationImagePath))
            {
                ImageDimensions dstDimension = _resizer.LoadImageInfo(stream);
                Assert.AreEqual(expectedWidth, dstDimension.Width);
                Assert.AreEqual(expectedHeight, dstDimension.Height);
            }
        }
    }
}
