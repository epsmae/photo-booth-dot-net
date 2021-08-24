using System.Collections.Generic;
using System.IO;

namespace PhotoBooth.Abstraction
{
    public interface IFileService
    {
        Stream OpenFile(string fileName);
        string PhotoDirectory { get; }
        List<FileInfo> AvailableImages { get; }
        void CleanupImageDirectory();
    }
}
