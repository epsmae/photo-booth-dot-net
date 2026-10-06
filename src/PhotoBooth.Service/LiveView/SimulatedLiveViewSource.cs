using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using PhotoBooth.Abstraction.LiveView;

namespace PhotoBooth.Service.LiveView
{
    /// <summary>
    /// Generates animated JPEG frames, used for development without a camera.
    /// Frame size and rate are similar to the Nikon D7000 live view.
    /// </summary>
    public class SimulatedLiveViewSource : ILiveViewSource
    {
        public bool SupportsCaptureDuringLiveView
        {
            get
            {
                // behaves like the gphoto2 command line live view
                return false;
            }
        }

        private const int Width = 640;
        private const int Height = 424;
        private const int FramesPerSecond = 15;

        public async Task RunAsync(Action<byte[]> onFrame, CancellationToken stopToken, CancellationToken killToken)
        {
            using (CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(stopToken, killToken))
            using (SimulatedFrameRenderer renderer = new SimulatedFrameRenderer(Width, Height))
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                TimeSpan frameDuration = TimeSpan.FromSeconds(1.0 / FramesPerSecond);
                long frameNumber = 0;

                while (!linked.IsCancellationRequested)
                {
                    onFrame(renderer.Render("LIVE VIEW SIMULATOR", ++frameNumber, 70));

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
