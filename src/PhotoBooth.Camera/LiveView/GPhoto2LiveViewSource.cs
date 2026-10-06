using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PhotoBooth.Abstraction.LiveView;

namespace PhotoBooth.Camera.LiveView
{
    /// <summary>
    /// Live view using "gphoto2 --capture-movie --stdout", which writes the camera preview frames
    /// as concatenated JPEG images (MJPEG) to stdout until it receives SIGINT.
    /// On SIGINT gphoto2 exits cleanly and ends the live view on the camera (mirror down), so the
    /// camera can be used for the next capture right away.
    /// </summary>
    public class GPhoto2LiveViewSource : ILiveViewSource
    {
        private readonly ILogger<GPhoto2LiveViewSource> _logger;
        private readonly LiveViewOptions _options;

        public GPhoto2LiveViewSource(ILogger<GPhoto2LiveViewSource> logger, IOptions<LiveViewOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        public async Task RunAsync(Action<byte[]> onFrame, CancellationToken stopToken, CancellationToken killToken)
        {
            List<string> arguments = new List<string>();

            if (!string.IsNullOrEmpty(_options.Camera))
            {
                arguments.Add("--camera");
                arguments.Add(_options.Camera);
            }

            arguments.Add("--capture-movie");
            arguments.Add("--stdout");

            MjpegFrameParser parser = new MjpegFrameParser(onFrame);
            StringBuilder standardError = new StringBuilder();

            Command command = Cli.Wrap(_options.GPhoto2Path)
                .WithArguments(arguments)
                .WithStandardOutputPipe(PipeTarget.Create((stream, token) => parser.ReadAsync(stream, token)))
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(standardError))
                .WithValidation(CommandResultValidation.None);

            _logger.LogInformation($"Starting live view: {_options.GPhoto2Path} {string.Join(" ", arguments)}");

            try
            {
                // the graceful token sends SIGINT, the forceful token kills the process
                CommandResult result = await command.ExecuteAsync(killToken, stopToken);

                _logger.LogInformation($"Live view process exited, code={result.ExitCode}, frames={parser.FrameCount}, discarded bytes={parser.DiscardedBytes}");

                if (parser.FrameCount == 0 || result.ExitCode != 0)
                {
                    throw new LiveViewException(BuildErrorMessage(standardError.ToString(), result.ExitCode));
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation($"Live view stopped, frames={parser.FrameCount}, discarded bytes={parser.DiscardedBytes}, stderr={standardError.ToString().Trim()}");
                throw;
            }
        }

        private static string BuildErrorMessage(string standardError, int exitCode)
        {
            string error = standardError.Trim();

            if (string.IsNullOrEmpty(error))
            {
                return $"gphoto2 live view ended without frames, exit code={exitCode}";
            }

            // e.g. "Liveview cannot start: Exposure Program Mode is not P/A/S/M"
            return error;
        }
    }
}
