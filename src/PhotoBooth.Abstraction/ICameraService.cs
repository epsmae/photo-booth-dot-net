using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoBooth.Abstraction
{
    public interface ICameraService
    {
        Task<CaptureResult> CaptureImage(string directory, string selectedCamera);
        Task<List<CameraInfo>> ListCameras();
        Task<StorageInfo> FetchStorageInfo();
        Task<CameraStatus> FetchCameraStatus();
        Task<byte[]> CaptureImageData(string directory, string selectedCamera);
        Task Initialize();
    }
}
