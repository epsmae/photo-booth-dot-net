using System;
using System.Diagnostics;
using SkiaSharp;

namespace PhotoBooth.Service.LiveView
{
    /// <summary>
    /// Renders animated test images (moving circle, time, frame number) as JPEG.
    /// </summary>
    public sealed class SimulatedFrameRenderer : IDisposable
    {
        private static readonly Stopwatch Clock = Stopwatch.StartNew();

        private readonly int _width;
        private readonly int _height;
        private readonly SKSurface _surface;
        private readonly SKPaint _circlePaint = new SKPaint {Color = SKColors.White.WithAlpha(180), IsAntialias = true};
        private readonly SKPaint _textPaint = new SKPaint {Color = SKColors.White, IsAntialias = true};
        private readonly SKFont _font;

        public SimulatedFrameRenderer(int width, int height)
        {
            _width = width;
            _height = height;
            _surface = SKSurface.Create(new SKImageInfo(width, height));
            _font = new SKFont {Size = height / 15f};
        }

        public byte[] Render(string title, long frameNumber, int quality)
        {
            double t = Clock.Elapsed.TotalSeconds;
            SKCanvas canvas = _surface.Canvas;

            canvas.Clear(SKColor.FromHsv((float) (t * 20 % 360), 60, 70));

            float x = (float) (_width / 2.0 + Math.Sin(t * 1.3) * _width / 3.0);
            float y = (float) (_height / 2.0 + Math.Cos(t * 0.9) * _height / 4.0);
            canvas.DrawCircle(x, y, _height / 8f, _circlePaint);

            canvas.DrawText(title, _width / 32f, _font.Size * 1.4f, SKTextAlign.Left, _font, _textPaint);
            canvas.DrawText($"{DateTime.Now:HH:mm:ss.f}  #{frameNumber}", _width / 32f, _height - _font.Size * 0.7f, SKTextAlign.Left, _font, _textPaint);

            using (SKImage image = _surface.Snapshot())
            using (SKData data = image.Encode(SKEncodedImageFormat.Jpeg, quality))
            {
                return data.ToArray();
            }
        }

        public void Dispose()
        {
            _surface.Dispose();
            _circlePaint.Dispose();
            _textPaint.Dispose();
            _font.Dispose();
        }
    }
}
