using System.IO;

namespace PhotoBooth.Abstraction
{
    public interface IFileProvider
    {
        Stream OpenFile(string fileName);
    }
}
