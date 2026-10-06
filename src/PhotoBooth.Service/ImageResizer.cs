using System;
using System.IO;
using PhotoBooth.Abstraction;
using SkiaSharp;

namespace PhotoBooth.Service
{
    public class ImageResizer : IImageResizer
    {
        private static readonly SKSamplingOptions ResizeSampling = new SKSamplingOptions(SKFilterMode.Linear);

        public byte[] ResizeImage(Stream fileStream, int expectedWidth, int expectedQuality)
        {
            using (SKCodec codec = CreateCodec(fileStream))
            {
                double scaleFactor = ((double) expectedWidth) / codec.Info.Width;
                int newWidth = (int) (codec.Info.Width * scaleFactor);
                int newHeight = (int) (codec.Info.Height * scaleFactor);

                using (SKBitmap srcBitmap = DecodeScaled(codec, newWidth, newHeight))
                using (SKBitmap resizedBitmap = srcBitmap.Resize(new SKSizeI(newWidth, newHeight), ResizeSampling))
                {
                    return resizedBitmap.Encode(SKEncodedImageFormat.Jpeg, expectedQuality).ToArray();
                }
            }
        }

        public ImageDimensions LoadImageInfo(Stream fileStream)
        {
            // only the image header is read, the pixels are not decoded
            using (SKCodec codec = CreateCodec(fileStream))
            {
                return new ImageDimensions
                {
                    Height = codec.Info.Height,
                    Width = codec.Info.Width
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

        internal static SKCodec CreateCodec(Stream stream)
        {
            SKCodec codec = SKCodec.Create(stream, out SKCodecResult result);

            if (codec == null)
            {
                throw new ArgumentException($"Unable to decode image, result={result}");
            }

            return codec;
        }

        /// <summary>
        /// Decodes the image with the smallest size the codec supports (JPEG: 1/8 steps)
        /// which is still at least <paramref name="minWidth"/> x <paramref name="minHeight"/>.
        /// Decoding a downscaled JPEG is a lot faster and needs less memory than decoding
        /// the full resolution image and resizing it afterwards.
        /// </summary>
        internal static SKBitmap DecodeScaled(SKCodec codec, int minWidth, int minHeight)
        {
            SKImageInfo info = codec.Info;

            float scale = Math.Max((float) minWidth / info.Width, (float) minHeight / info.Height);

            if (scale < 1)
            {
                SKSizeI scaledSize = codec.GetScaledDimensions(scale);

                if (scaledSize.Width >= minWidth && scaledSize.Height >= minHeight)
                {
                    info = info.WithSize(scaledSize.Width, scaledSize.Height);
                }
            }

            if (info.AlphaType == SKAlphaType.Unpremul)
            {
                info = info.WithAlphaType(SKAlphaType.Premul);
            }

            SKBitmap bitmap = SKBitmap.Decode(codec, info);

            if (bitmap == null)
            {
                throw new ArgumentException("Unable to decode image");
            }

            return bitmap;
        }
    }
}
