using System.Collections.Generic;

namespace PhotoBooth.Abstraction
{
    public interface IImageCombiner
    {
        /// <summary>
        /// Combines images the destination path is returned to handle the case
        /// where not the one from the parameters is used
        /// </summary>
        /// <param name="offsetCalculator"></param>
        /// <param name="imageFilePaths"></param>
        /// <param name="destinationPath"></param>
        /// <returns></returns>
        string Combine(IImageGalleryOffsetCalculator offsetCalculator, IList<string> imageFilePaths, string destinationPath);
    }
}
