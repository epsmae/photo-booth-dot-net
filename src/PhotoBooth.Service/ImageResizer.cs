using System.IO;
using PhotoBooth.Abstraction;
using SkiaSharp;

namespace PhotoBooth.Service
{
    public class ImageResizer : IImageResizer
    {

        public byte[] ResizeImage(Stream fileStream, int expectedWidth, int expectedQuality)
        {
            using (SKBitmap srcBitmap = SKBitmap.Decode(fileStream))
            {

                double scaleFactor = ((double) expectedWidth) / srcBitmap.Width;
                int newWidth = (int) (srcBitmap.Width * scaleFactor);
                int newHeight = (int) (srcBitmap.Height * scaleFactor);
                using (SKBitmap resizedBitmap =
                    srcBitmap.Resize(new SKSizeI(newWidth, newHeight), SKFilterQuality.Low))
                {
                    return resizedBitmap.Encode(SKEncodedImageFormat.Jpeg, expectedQuality).ToArray();
                }
            }
        }

        public ImageDimensions LoadImageInfo(Stream fileStream)
        {
            using (SKBitmap srcBitmap = SKBitmap.Decode(fileStream))
            {
                return new ImageDimensions
                {
                    Height = srcBitmap.Height,
                    Width = srcBitmap.Width
                };
            }
        }

        public ImageDimensions LoadImageInfo(string imageFilePath)
        {
            using (FileStream fileStream = File.OpenRead(imageFilePath))
            {
                return LoadImageInfo(fileStream);
            }
        }
    }
}
