using System.IO;
using System.Reflection;
using PhotoBooth.Abstraction;

namespace PhotoBooth.Service
{
    public class FilePathProvider : IFilePathProvider
    {
        public string ExecutionDirectory
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            }
        }
    }
}
