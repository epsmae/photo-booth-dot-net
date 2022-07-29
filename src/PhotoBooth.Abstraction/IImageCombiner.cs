using System.Collections.Generic;

namespace PhotoBooth.Abstraction
{
    public interface IImageCombiner
    {
        /// <summary>
        /// Combine images the destination path is returned to handle the case
        /// where not the one from the parameters is used
        /// </summary>
        /// <param name="imageFilePaths"></param>
        /// <param name="destinationPath"></param>
        /// <returns></returns>
        string Combine(IList<string> imageFilePaths, string destinationPath);
    }
}
