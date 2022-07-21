using System.Collections.Generic;
using PhotoBooth.Abstraction;
using SkiaSharp;

namespace PhotoBooth.Service
{
    public class ImageCombiner : IImageCombiner
    {
        // https://github.com/mattleibow/SkiaSharpImageMerger/blob/master/SkiaSharpImageMerger/Program.cs

        public void Combine(string image1SrcPath, string image2SrcPath, string destinationPath)
        {
            IList<string> paths = new List<string>();
            paths.Add(image1SrcPath);
            paths.Add(image2SrcPath);

            using (SKSurface tempSurface = SKSurface.Create(new SKImageInfo(2464, 1632)))
            {
                //get the drawing canvas of the surface
                SKCanvas canvas = tempSurface.Canvas;

                //set background color
                canvas.Clear(SKColors.White);

                //go through each image and draw it on the final image
                int offset = 0;
                int offsetTop = 0;
                foreach (SKBitmap image in images)
                {
                    canvas.DrawBitmap(image, SKRect.Create(offset, offsetTop, image.Width, image.Height));
                    offsetTop = offsetTop > 0 ? 0 : image.Height / 2;
                    offset += (int) (image.Width / 1.6);
                }

                // return the surface as a manageable image
                finalImage = tempSurface.Snapshot();
            }




            using (SKBitmap srcBitmap = new SKBitmap(new SKImageInfo(2464, 1632)))
            {
                srcBitmap.

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
    }
}
