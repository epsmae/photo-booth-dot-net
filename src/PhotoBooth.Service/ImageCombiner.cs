using System;
using System.Collections.Generic;
using System.IO;
using PhotoBooth.Abstraction;
using SkiaSharp;

namespace PhotoBooth.Service
{
    public class ImageCombiner : IImageCombiner
    {
        private readonly IImageGalleryOffsetCalculator _offsetCalculator;

        public ImageCombiner(IImageGalleryOffsetCalculator offsetCalculator)
        {
            _offsetCalculator = offsetCalculator;
        }

        public void Combine(IList<string> imageFilePaths,  string destinationPath)
        {
            if (imageFilePaths.Count != _offsetCalculator.RequiredImageCount)
            {
                throw new ArgumentException($"imageFilePathCount expected={_offsetCalculator.RequiredImageCount}, actual={imageFilePaths.Count}");
            }

            using (SKSurface tempSurface = SKSurface.Create(new SKImageInfo(2464, 1632)))
            {
                SKCanvas canvas = tempSurface.Canvas;

                canvas.Clear(SKColors.White);
                
                for (int i = 0; i < imageFilePaths.Count; i++)
                {
                    using (SKBitmap bitmap = SKBitmap.Decode(imageFilePaths[i]))
                    {
                        ImageOffsetInfo info = _offsetCalculator.GetOffset(i, bitmap.Width, bitmap.Height);
                        
                        using (SKBitmap resizedBitmap = bitmap.Resize(new SKSizeI((int) info.Width, (int) info.Height), SKFilterQuality.Low))
                        {
                            canvas.DrawBitmap(resizedBitmap, SKRect.Create((int) info.LeftOffset, (int) info.TopOffset, (int) info.Width, (int) info.Height));
                        }
                    }
                }

                using (SKImage finalImage = tempSurface.Snapshot())
                {
                    using (SKData encoded = finalImage.Encode(SKEncodedImageFormat.Jpeg, 80))
                    {
                        using (Stream outFile = File.OpenWrite(destinationPath))
                        {
                            encoded.SaveTo(outFile);
                        }
                    }
                }
            }
        }
    }
}
