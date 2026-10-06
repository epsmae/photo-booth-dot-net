using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using PhotoBooth.Abstraction.LiveView;
using SkiaSharp;

namespace PhotoBooth.Service.LiveView
{
    /// <summary>
    /// Generates animated JPEG frames, used for development without a camera.
    /// Frame size and rate are similar to the Nikon D7000 live view.
    /// </summary>
    public class SimulatedLiveViewSource : ILiveViewSource
    {
        private const int Width = 640;
        private const int Height = 424;
        private const int FramesPerSecond = 15;

        public async Task RunAsync(Action<byte[]> onFrame, CancellationToken stopToken, CancellationToken killToken)
        {
            using (CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(stopToken, killToken))
            using (SKSurface surface = SKSurface.Create(new SKImageInfo(Width, Height)))
            using (SKPaint circlePaint = new SKPaint { Color = SKColors.White.WithAlpha(180), IsAntialias = true })
            using (SKFont font = new SKFont { Size = 28 })
            using (SKPaint textPaint = new SKPaint { Color = SKColors.White, IsAntialias = true })
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                TimeSpan frameDuration = TimeSpan.FromSeconds(1.0 / FramesPerSecond);
                long frameNumber = 0;

                while (!linked.IsCancellationRequested)
                {
                    double t = stopwatch.Elapsed.TotalSeconds;
                    SKCanvas canvas = surface.Canvas;

                    canvas.Clear(SKColor.FromHsv((float) (t * 20 % 360), 60, 70));

                    float x = (float) (Width / 2.0 + Math.Sin(t * 1.3) * Width / 3.0);
                    float y = (float) (Height / 2.0 + Math.Cos(t * 0.9) * Height / 4.0);
                    canvas.DrawCircle(x, y, 50, circlePaint);

                    canvas.DrawText("LIVE VIEW SIMULATOR", 20, 40, SKTextAlign.Left, font, textPaint);
                    canvas.DrawText($"{DateTime.Now:HH:mm:ss.f}  #{++frameNumber}", 20, Height - 20, SKTextAlign.Left, font, textPaint);

                    using (SKImage image = surface.Snapshot())
                    using (SKData data = image.Encode(SKEncodedImageFormat.Jpeg, 70))
                    {
                        onFrame(data.ToArray());
                    }

                    TimeSpan wait = TimeSpan.FromTicks(frameDuration.Ticks * frameNumber) - stopwatch.Elapsed;

                    if (wait > TimeSpan.Zero)
                    {
                        try
                        {
                            await Task.Delay(wait, linked.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                    }
                }
            }

            stopToken.ThrowIfCancellationRequested();
            killToken.ThrowIfCancellationRequested();
        }
    }
}
