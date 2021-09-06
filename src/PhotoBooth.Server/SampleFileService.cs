using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using PhotoBooth.Abstraction;
using FileInfo = PhotoBooth.Abstraction.FileInfo;

namespace PhotoBooth.Server
{
    public class SampleFileService : IFileService
    {
        private const string SampleImage = "SampleImage.jpg";
        private readonly string _rootDirectory;

        public SampleFileService()
        {
            _rootDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            if (!Directory.Exists(PhotoDirectory))
            {
                Directory.CreateDirectory(PhotoDirectory);
            }

            File.Copy(SampleImage, Path.Combine(PhotoDirectory, SampleImage), true);
        }


        public Stream OpenFile(string fileName)
        {
            return File.OpenRead(SampleImage);
        }

        public string PhotoDirectory
        {
            get
            {
                return Path.Combine(_rootDirectory, "Images");
            }
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

                    items.Add(new FileInfo { FullFileName = file, Name = Path.GetFileNameWithoutExtension(file) });
                }

                return items;
            }
        }

        public void CleanupImageDirectory()
        {
            
        }
    }
}
