using System.Threading.Tasks;

namespace PhotoBooth.Abstraction
{
    public interface ICameraAdapter
    {
        Task<CommandLineResult> Capture(string fileName);
        Task<CommandLineResult> ListCameras();
        Task<CommandLineResult> GetStorageInfo();
        Task<CommandLineResult> GetCameraStatus();
        Task<CommandLineResult> GetBatteryInfo();
        Task<CommandLineResult> Initialize();
    }
}
