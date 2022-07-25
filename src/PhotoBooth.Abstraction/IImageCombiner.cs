using System.Collections.Generic;

namespace PhotoBooth.Abstraction
{
    public interface IImageCombiner
    {
        void Combine(IList<string> imageFilePaths, string destinationPath);
    }
}
