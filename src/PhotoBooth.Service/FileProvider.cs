using System.IO;
using PhotoBooth.Abstraction;

namespace PhotoBooth.Service
{
    public class FileProvider : IFileProvider
    {
        public Stream OpenFile(string fileName)
        {
            return File.OpenRead(fileName);
        }
    }
}
