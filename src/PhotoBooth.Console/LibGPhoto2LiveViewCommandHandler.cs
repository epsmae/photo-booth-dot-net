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
using Microsoft.Extensions.Options;
using PhotoBooth.Abstraction;
using PhotoBooth.Abstraction.LibGPhoto2;
using PhotoBooth.Camera.LibGPhoto2;
using PhotoBooth.Service.LiveView;

namespace PhotoBooth.Console
{
    /// <summary>
    /// Hardware test for the libgphoto2 live view: measures the frame rate and how long the live view
    /// is interrupted by a capture (captures are taken out of the running live view).
    /// </summary>
    public class LibGPhoto2LiveViewCommandHandler
    {
        private readonly ILogger<LibGPhoto2LiveViewCommandHandler> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IImageResizer _imageResizer;

        public LibGPhoto2LiveViewCommandHandler(ILogger<LibGPhoto2LiveViewCommandHandler> logger, ILoggerFactory loggerFactory, IImageResizer imageResizer)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
            _imageResizer = imageResizer;
        }

        public IList<Command> BuildCommands()
        {
            Option<int> secondsOption = new Option<int>("--seconds") {Description = "Live view duration before the first capture", DefaultValueFactory = _ => 10};
            Option<int> capturesOption = new Option<int>("--captures") {Description = "Number of captures out of the live view", DefaultValueFactory = _ => 3};
            Option<CameraFocusMode> focusOption = new Option<CameraFocusMode>("--focus") {Description = "Focus mode before the capture", DefaultValueFactory = _ => CameraFocusMode.None};
            Option<string> outputOption = new Option<string>("--output") {Description = "Directory for the captured images and live view frames (optional)"};
#if DEBUG
            Option<bool> simulateOption = new Option<bool>("--simulate") {Description = "Use a simulated camera", DefaultValueFactory = _ => true};
#else
            Option<bool> simulateOption = new Option<bool>("--simulate") {Description = "Use a simulated camera", DefaultValueFactory = _ => false};
#endif

            Command command = new Command("liveview-libgphoto2", "Live view and captures via libgphoto2 (one camera connection), prints timing statistics")
            {
                secondsOption,
                capturesOption,
                focusOption,
                outputOption,
                simulateOption
            };

            command.SetAction(async (parseResult, cancellationToken) => await Run(
                parseResult.GetValue(secondsOption),
                parseResult.GetValue(capturesOption),
                parseResult.GetValue(focusOption),
                parseResult.GetValue(outputOption),
                parseResult.GetValue(simulateOption)));

            return new List<Command> {command};
        }

        private async Task<int> Run(int seconds, int captures, CameraFocusMode focusMode, string outputDirectory, bool simulate)
        {
            string directory = string.IsNullOrEmpty(outputDirectory) ? Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) : outputDirectory;
            Directory.CreateDirectory(directory);

            IGPhoto2Api api;

            try
            {
                api = simulate ? new SimulatedGPhoto2Api() : new GPhoto2Api();
            }
            catch (DllNotFoundException ex)
            {
                _logger.LogError(ex.Message);
                return ResultCodes.Error;
            }

            CameraDriverOptions options = new CameraDriverOptions {FocusMode = focusMode};

            using (CameraSession session = new CameraSession(api, _loggerFactory.CreateLogger<CameraSession>(), Options.Create(options)))
            {
                Stopwatch clock = Stopwatch.StartNew();
                List<double> frameTimes = new List<double>();
                object frameLock = new object();
                byte[] firstFrame = null;

                try
                {
                    await session.ConnectAsync();
                    _logger.LogInformation($"Connected:         {clock.ElapsedMilliseconds} ms, {await session.GetModelAsync()}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Connect failed: {ex.Message}");
                    return ResultCodes.Error;
                }

                TaskCompletionSource<Exception> previewError = new TaskCompletionSource<Exception>();
                double previewStart = clock.Elapsed.TotalMilliseconds;

                session.StartPreview(frame =>
                {
                    lock (frameLock)
                    {
                        frameTimes.Add(clock.Elapsed.TotalMilliseconds);
                        firstFrame ??= frame;

                        if (!string.IsNullOrEmpty(outputDirectory) && frameTimes.Count <= 50)
                        {
                            File.WriteAllBytes(Path.Combine(outputDirectory, $"frame_{frameTimes.Count:D5}.jpg"), frame);
                        }
                    }
                }, ex => previewError.TrySetResult(ex));

                if (await Task.WhenAny(previewError.Task, Task.Delay(TimeSpan.FromSeconds(seconds))) == previewError.Task)
                {
                    _logger.LogError($"Live view failed: {previewError.Task.Result.Message}");
                    return ResultCodes.Error;
                }

                double[] liveViewFrames;

                lock (frameLock)
                {
                    liveViewFrames = frameTimes.ToArray();
                }

                if (liveViewFrames.Length < 2)
                {
                    _logger.LogError("No live view frames received");
                    return ResultCodes.Error;
                }

                ImageDimensions dimensions = _imageResizer.LoadImageInfo(new MemoryStream(firstFrame));
                double fps = (liveViewFrames.Length - 1) / ((liveViewFrames.Last() - liveViewFrames.First()) / 1000.0);

                _logger.LogInformation($"First frame after: {liveViewFrames.First() - previewStart:F0} ms");
                _logger.LogInformation($"Resolution:        {dimensions.Width}x{dimensions.Height}");
                _logger.LogInformation($"Frames per second: {fps:F1}");

                for (int i = 1; i <= captures; i++)
                {
                    string file = Path.Combine(directory, $"capture_{DateTime.Now:HHmmss_fff}.jpg");
                    double start = clock.Elapsed.TotalMilliseconds;

                    try
                    {
                        await session.CaptureImageAsync(file);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Capture {i} failed: {ex.Message}");
                        return ResultCodes.Error;
                    }

                    double end = clock.Elapsed.TotalMilliseconds;

                    // wait for the live view to come back
                    await Task.Delay(1500);

                    double[] frames;

                    lock (frameLock)
                    {
                        frames = frameTimes.ToArray();
                    }

                    double lastBefore = frames.Where(t => t <= start).DefaultIfEmpty(start).Max();
                    double firstAfter = frames.Where(t => t >= end).DefaultIfEmpty(double.NaN).Min();

                    _logger.LogInformation($"Capture {i}: capture + download {end - start:F0} ms, live view gap {firstAfter - lastBefore:F0} ms, " +
                                           $"live view back {firstAfter - end:F0} ms after the download ({file})");
                }

                session.StopPreview();
            }

            return ResultCodes.Success;
        }
    }
}
