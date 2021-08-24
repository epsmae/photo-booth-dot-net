using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using PhotoBooth.Abstraction;
using FileInfo = PhotoBooth.Abstraction.FileInfo;

namespace PhotoBooth.Service.Test
{
    public class FileServiceMock : IFileService
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

        public string PhotoDirectory
        {
            get
            {
                return Path.Combine(TestContext.CurrentContext.TestDirectory, "Images");
            }
        }

        public List<FileInfo> AvailableImages
        {
            get
            {
                return new List<FileInfo>();
            }
        }

        public void CleanupImageDirectory()
        {
            
        }
    }
}
