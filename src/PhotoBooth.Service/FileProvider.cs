using System.IO;
using System.Reflection;
using PhotoBooth.Abstraction;

namespace PhotoBooth.Service
{
    public class FileProvider : IFileProvider
    {
        private readonly string _rootDirectory;

        public FileProvider()
        {
            _rootDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        }

        public string PhotoDirectory
        {
            get
            {
                return Path.Combine(_rootDirectory, "Images");
            }
        }

        public Stream OpenFile(string fileName)
        {
            return File.OpenRead(fileName);
        }
    }
}
