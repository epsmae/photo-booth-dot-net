using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using Microsoft.Extensions.Logging;
using PhotoBooth.Abstraction;

namespace PhotoBooth.Camera
{
    public class GPhoto2CameraAdapter : ICameraAdapter
    {
        private readonly ILogger<GPhoto2CameraAdapter> _logger;
        private const string GPhotoExe = "gphoto2";

        public GPhoto2CameraAdapter(ILogger<GPhoto2CameraAdapter> logger)
        {
            _logger = logger;
        }

        public async Task<CommandLineResult> Capture(string selectedCamera, string fileName)
        {
            BufferedCommandResult result = await Cli.Wrap(GPhotoExe)
                .WithArguments($"--camera \"{selectedCamera}\" --filename \"{fileName}\" --keep --capture-image-and-download")
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();

            return MapResult(result);
        }

        public async Task<CommandLineResult> ListCameras()
        {
            BufferedCommandResult result = await Cli.Wrap(GPhotoExe)
                .WithArguments("--auto-detect")
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();

            return MapResult(result);
        }

        public async Task<CommandLineResult> GetBatteryInfo()
        {
            BufferedCommandResult result = await Cli.Wrap(GPhotoExe)
                .WithArguments("--get-config batterylevel")
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();

            return MapResult(result);
        }

        public async Task<CommandLineResult> Initialize()
        {
            BufferedCommandResult result = await Cli.Wrap("pkill")
                .WithArguments("--f gphoto2")
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();

            return MapResult(result);
        }

        public async Task<CommandLineResult> Configure()
        {
            BufferedCommandResult result = await Cli.Wrap(GPhotoExe)
                .WithArguments("--set-config capturetarget=1")
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();

            return MapResult(result);
        }


        public async Task<CommandLineResult> GetStorageInfo()
        {

            BufferedCommandResult result = await Cli.Wrap(GPhotoExe)
                .WithArguments("--storage-info")
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();

            return MapResult(result);
        }

        public async Task<CommandLineResult> GetCameraStatus()
        {
            BufferedCommandResult result = await Cli.Wrap(GPhotoExe)
                .WithArguments("--summary")
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();

            return MapResult(result);
        }

        private static CommandLineResult MapResult(BufferedCommandResult result)
        {
            return new CommandLineResult
            {
                ExitCode = result.ExitCode,
                StandardError = result.StandardError,
                StandardOutput = result.StandardOutput
            };
        }
    }
}
