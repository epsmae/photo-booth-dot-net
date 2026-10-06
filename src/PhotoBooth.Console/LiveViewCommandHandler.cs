using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PhotoBooth.Abstraction;
using PhotoBooth.Abstraction.LiveView;

namespace PhotoBooth.Console
{
    /// <summary>
    /// Hardware test for the live view: measures frame rate, frame size, stop duration and the
    /// time to switch from live view to a capture.
    /// </summary>
    public class LiveViewCommandHandler
    {
        private const int StopTimeoutMilliseconds = 5000;

        private readonly ILogger<LiveViewCommandHandler> _logger;
        private readonly ILiveViewSource _source;
        private readonly ICameraService _cameraService;
        private readonly IImageResizer _imageResizer;

        public LiveViewCommandHandler(ILogger<LiveViewCommandHandler> logger, ILiveViewSource source, ICameraService cameraService, IImageResizer imageResizer)
        {
            _logger = logger;
            _source = source;
            _cameraService = cameraService;
            _imageResizer = imageResizer;
        }

        public IList<Command> BuildCommands()
        {
            Option<int> secondsOption = new Option<int>("--seconds") {Description = "Live view duration in seconds", DefaultValueFactory = _ => 10};
            Option<string> outputOption = new Option<string>("--output") {Description = "Directory to store the received frames (optional)"};
            Option<bool> captureOption = new Option<bool>("--capture") {Description = "Capture an image after the live view stopped and measure the switch time"};

            Command liveViewCommand = new Command("liveview", "Run the camera live view and print statistics")
            {
                secondsOption,
                outputOption,
                captureOption
            };

            liveViewCommand.SetAction(async (parseResult, cancellationToken) =>
                await RunLiveView(parseResult.GetValue(secondsOption), parseResult.GetValue(outputOption), parseResult.GetValue(captureOption)));

            return new List<Command> {liveViewCommand};
        }

        private async Task<int> RunLiveView(int seconds, string outputDirectory, bool capture)
        {
            if (!string.IsNullOrEmpty(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            using (CancellationTokenSource stopCts = new CancellationTokenSource())
            using (CancellationTokenSource killCts = new CancellationTokenSource())
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                long frames = 0;
                long totalBytes = 0;
                double firstFrameMs = -1;
                byte[] firstFrame = null;

                Task run = _source.RunAsync(frame =>
                {
                    long frameNumber = Interlocked.Increment(ref frames);
                    Interlocked.Add(ref totalBytes, frame.Length);

                    if (frameNumber == 1)
                    {
                        firstFrameMs = stopwatch.Elapsed.TotalMilliseconds;
                        firstFrame = frame;
                    }

                    if (!string.IsNullOrEmpty(outputDirectory))
                    {
                        File.WriteAllBytes(Path.Combine(outputDirectory, $"frame_{frameNumber:D5}.jpg"), frame);
                    }
                }, stopCts.Token, killCts.Token);

                _logger.LogInformation($"Live view running for {seconds}s...");
                await Task.WhenAny(run, Task.Delay(TimeSpan.FromSeconds(seconds)));
                double liveViewEndMs = stopwatch.Elapsed.TotalMilliseconds;

                Stopwatch stopStopwatch = Stopwatch.StartNew();
                stopCts.Cancel();

                if (await Task.WhenAny(run, Task.Delay(StopTimeoutMilliseconds)) != run)
                {
                    _logger.LogWarning($"Live view did not stop within {StopTimeoutMilliseconds}ms after SIGINT, killing it");
                    killCts.Cancel();
                }

                try
                {
                    await run;
                }
                catch (OperationCanceledException)
                {
                    // expected, stopped by us
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Live view failed");
                    return ResultCodes.Error;
                }

                double stopMs = stopStopwatch.Elapsed.TotalMilliseconds;

                if (frames == 0)
                {
                    _logger.LogError("No live view frames received");
                    return ResultCodes.Error;
                }

                double fps = frames > 1 ? (frames - 1) / ((liveViewEndMs - firstFrameMs) / 1000.0) : 0;
                ImageDimensions dimensions = _imageResizer.LoadImageInfo(new MemoryStream(firstFrame));

                _logger.LogInformation($"Frames:            {frames}");
                _logger.LogInformation($"Resolution:        {dimensions.Width}x{dimensions.Height}");
                _logger.LogInformation($"Frames per second: {fps:F1}");
                _logger.LogInformation($"Average size:      {totalBytes / frames / 1024} KB ({totalBytes * 8.0 / 1_000_000 / ((liveViewEndMs - firstFrameMs) / 1000.0):F1} Mbit/s)");
                _logger.LogInformation($"First frame after: {firstFrameMs:F0} ms");
                _logger.LogInformation($"Stop duration:     {stopMs:F0} ms");

                if (capture)
                {
                    return await CaptureAfterLiveView(stopMs);
                }

                return ResultCodes.Success;
            }
        }

        private async Task<int> CaptureAfterLiveView(double stopMs)
        {
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                List<CameraInfo> cameras = await _cameraService.ListCameras();
                string directory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                CaptureResult result = await _cameraService.CaptureImage(directory, cameras.First().CameraModel);
                double captureMs = stopwatch.Elapsed.TotalMilliseconds;

                _logger.LogInformation($"Capture duration:  {captureMs:F0} ms ({result.FileName})");
                _logger.LogInformation($"Live view -> photo downloaded: {stopMs + captureMs:F0} ms");
                return ResultCodes.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Capture after live view failed");
                return ResultCodes.Error;
            }
        }
    }
}
