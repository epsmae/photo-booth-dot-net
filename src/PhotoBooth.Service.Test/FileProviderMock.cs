using System.IO;
using NUnit.Framework;
using PhotoBooth.Abstraction;

namespace PhotoBooth.Service.Test
{
    public class FileProviderMock : IFileProvider
    {
        private string SampleImagePath
        {
            get
            {
                return Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "SampleImage.jpg");
            }
        }
        
        public Stream OpenFile(string fileName)
        {
            return File.OpenRead(SampleImagePath);
        }
    }
}
