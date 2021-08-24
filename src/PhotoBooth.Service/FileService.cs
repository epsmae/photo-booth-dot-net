using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using PhotoBooth.Abstraction;
using FileInfo = PhotoBooth.Abstraction.FileInfo;

namespace PhotoBooth.Service
{
    public class FileService : IFileService
    {
        private readonly string _rootDirectory;

        public FileService()
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


        public List<FileInfo> AvailableImages
        {
            get
            {
                List<FileInfo> items = new List<FileInfo>();

                if (!Directory.Exists(PhotoDirectory))
                {
                    return items;
                }

                foreach (string file in Directory.EnumerateFiles(PhotoDirectory))
                {
                    
                    items.Add(new FileInfo{FullFileName = file, Name = Path.GetFileNameWithoutExtension(file)});
                }

                return items;
            }
        }

        public void CleanupImageDirectory()
        {
            if (Directory.Exists(PhotoDirectory))
            {
                Directory.Delete(PhotoDirectory, true);
            }
        }
    }
}
