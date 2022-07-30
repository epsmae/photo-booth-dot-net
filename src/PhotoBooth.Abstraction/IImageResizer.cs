using System.IO;

namespace PhotoBooth.Abstraction
{
    public interface IImageResizer
    {
        ImageDimensions LoadImageInfo(Stream fileStream);

        ImageDimensions LoadImageInfo(string imageFilePath);

        byte[] ResizeImage(Stream fileStream, int expectedWidth, int expectedQuality);
    }
}
