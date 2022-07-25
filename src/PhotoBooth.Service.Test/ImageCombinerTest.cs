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
            _combiner = new ImageCombiner(new FourImageGalleryCalculator());
        }

        [Test]
        public void TestCombine()
        {
            IList<string> items = new List<string>();
            items.Add(SourceImagePath);
            items.Add(SourceImagePath);
            items.Add(SourceImagePath);
            items.Add(SourceImagePath);

            _combiner.Combine(items, DestinationImagePath);
        }
    }
}
