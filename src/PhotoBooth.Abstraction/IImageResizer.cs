using System.IO;

namespace PhotoBooth.Abstraction
{
    public interface IImageResizer
    {
        ImageDimensions LoadImageInfo(Stream fileStream);


        byte[] ResizeImage(Stream fileStream, int expectedWidth);
    }
}
