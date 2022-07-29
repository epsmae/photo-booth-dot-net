using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace PhotoBooth.Service.Test
{
    public class ImageCombinerTest
    {
        private ImageCombiner _combiner;

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
                return Path.Combine(TestContext.CurrentContext.WorkDirectory, "SampleImageCombined.jpg");
            }
        }

        [SetUp]
        public void Setup()
        {
            _combiner = new ImageCombiner(new FourImageGalleryCalculator(), new FileServiceMock());
        }

        [Test]
        public void TestCombine()
        {
            IList<string> items = new List<string>();
            items.Add("file1.png");
            items.Add("file2.png");
            items.Add("file3.png");
            items.Add("file4.png");

            _combiner.Combine(items, DestinationImagePath);
        }
    }
}
