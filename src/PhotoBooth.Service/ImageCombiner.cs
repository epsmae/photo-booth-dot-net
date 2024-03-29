using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PhotoBooth.Abstraction;
using SkiaSharp;

namespace PhotoBooth.Service
{
    public class ImageCombiner : IImageCombiner
    {
        private readonly IFileService _fileService;
        private readonly IImageResizer _imageResizer;

        public ImageCombiner(IFileService fileService, IImageResizer imageResizer)
        {
            _fileService = fileService;
            _imageResizer = imageResizer;
        }

        public string Combine(IImageGalleryOffsetCalculator offsetCalculator, IList<string> imageFilePaths, string destinationPath)
        {
            if (imageFilePaths.Count != offsetCalculator.RequiredImageCount)
            {
                throw new ArgumentException($"imageFilePathCount expected={offsetCalculator.RequiredImageCount}, actual={imageFilePaths.Count}");
            }

            if (imageFilePaths.Count == 1)
            {
                // nothing to do
                return imageFilePaths.First();
            }

            ImageDimensions dimensions = _imageResizer.LoadImageInfo(imageFilePaths.First());

            using (SKSurface tempSurface = SKSurface.Create(new SKImageInfo(dimensions.Width, dimensions.Height)))
            {
                SKCanvas canvas = tempSurface.Canvas;

                canvas.Clear(SKColors.White);
                
                for (int i = 0; i < imageFilePaths.Count; i++)
                {
                    using (Stream stream = _fileService.OpenFile(imageFilePaths[i]))
                    {
                        using (SKBitmap bitmap = SKBitmap.Decode(stream))
                        {
                            ImageOffsetInfo info = offsetCalculator.GetOffset(i, bitmap.Width, bitmap.Height);

                            using (SKBitmap resizedBitmap = bitmap.Resize(new SKSizeI((int) info.Width, (int) info.Height), SKFilterQuality.Low))
                            {
                                canvas.DrawBitmap(resizedBitmap, SKRect.Create((int) info.LeftOffset, (int) info.TopOffset, (int) info.Width, (int) info.Height));
                            }
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

            return destinationPath;
        }
    }
}
