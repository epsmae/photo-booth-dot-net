using System.IO;
using System.Reflection;
using PhotoBooth.Abstraction;

namespace PhotoBooth.Server
{
    public class SampleFileProvider : IFileProvider
    {
        private readonly string _rootDirectory;

        public SampleFileProvider()
        {
            _rootDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        }


        public Stream OpenFile(string fileName)
        {
            return File.OpenRead("SampleImage.jpg");
        }

        public string PhotoDirectory
        {
            get
            {
                return Path.Combine(_rootDirectory, "Images");
            }
        }
    }
}
