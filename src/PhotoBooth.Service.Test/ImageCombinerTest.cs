using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using PhotoBooth.Abstraction;

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
            _combiner = new ImageCombiner(new FileService(), new ImageResizer());
        }

        [Test]
        public void TestCombine()
        {
            IList<string> items = new List<string>();
            items.Add(SourceImagePath);
            items.Add(SourceImagePath);
            items.Add(SourceImagePath);
            items.Add(SourceImagePath);

            _combiner.Combine(new FourImageGalleryCalculator(), items, DestinationImagePath);

            Assert.True(File.Exists(DestinationImagePath));
        }

        [Test]
        public void TestCombineOverwritesExistingFile()
        {
            IList<string> items = new List<string> {SourceImagePath, SourceImagePath, SourceImagePath, SourceImagePath};

            // existing file larger than the result must not leave trailing data behind
            File.WriteAllBytes(DestinationImagePath, new byte[20 * 1024 * 1024]);

            _combiner.Combine(new FourImageGalleryCalculator(), items, DestinationImagePath);

            ImageResizer resizer = new ImageResizer();
            ImageDimensions source = resizer.LoadImageInfo(SourceImagePath);
            ImageDimensions combined = resizer.LoadImageInfo(DestinationImagePath);

            Assert.AreEqual(source.Width, combined.Width);
            Assert.AreEqual(source.Height, combined.Height);
            Assert.Less(new System.IO.FileInfo(DestinationImagePath).Length, 20 * 1024 * 1024);
        }
    }
}
