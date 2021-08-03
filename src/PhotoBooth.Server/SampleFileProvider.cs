using System.IO;
using PhotoBooth.Abstraction;

namespace PhotoBooth.Server
{
    public class SampleFileProvider : IFileProvider
    {
        public Stream OpenFile(string fileName)
        {
            return File.OpenRead("SampleImage.jpg");
        }
    }
}
