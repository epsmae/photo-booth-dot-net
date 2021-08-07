using System.IO;
using PhotoBooth.Abstraction;

namespace PhotoBooth.Server
{
    public class SampleFileProvider : IFileProvider
    {
        public Stream OpenFile(string fileName)
        {
            if (File.Exists("SampleImage.jpg"))
            {

            }


            return File.OpenRead("SampleImage.jpg");
        }
    }
}
