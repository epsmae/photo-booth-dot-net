using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PhotoBooth.Abstraction;
using PhotoBooth.Abstraction.Exceptions;
using PhotoBooth.Abstraction.LibGPhoto2;

namespace PhotoBooth.Camera.LibGPhoto2
{
    /// <summary>
    /// Camera service using libgphoto2 in process (shares the camera connection with the live view).
    /// </summary>
    public class LibGPhoto2CameraService : ICameraService
    {
        private readonly ILogger<LibGPhoto2CameraService> _logger;
        private readonly CameraSession _session;

        public LibGPhoto2CameraService(ILogger<LibGPhoto2CameraService> logger, CameraSession session)
        {
            _logger = logger;
            _session = session;
        }

        public Task Initialize()
        {
            // no "pkill gphoto2" like the command line version, the camera connection is kept open
            return Map(_session.ConnectAsync());
        }

        public Task Configure()
        {
            // capture target is configured on every connect, see CameraDriverOptions.CaptureTarget
            return Task.CompletedTask;
        }

        public async Task<byte[]> CaptureImageData(string directory, string selectedCamera)
        {
            CaptureResult result = await CaptureImage(directory, selectedCamera);
            return await File.ReadAllBytesAsync(result.FileName);
        }

        public async Task<CaptureResult> CaptureImage(string directory, string selectedCamera)
        {
            string fileName = Path.Combine(directory, $"img_{DateTime.Now:dd-MM-yyyy_HH_mm_ss_fff}.jpg");
            _logger.LogInformation($"Capture image, file name={fileName}");

            await Map(_session.CaptureImageAsync(fileName));

            return new CaptureResult
            {
                FileName = fileName
            };
        }

        public async Task<List<CameraInfo>> ListCameras()
        {
            try
            {
                string model = await _session.GetModelAsync();
                return new List<CameraInfo> {new CameraInfo {CameraModel = model, Port = "usb"}};
            }
            catch (GPhoto2Exception ex) when (ex.IsConnectionError)
            {
                _logger.LogInformation($"No camera available: {ex.Message}");
                return new List<CameraInfo>();
            }
        }

        public Task<StorageInfo> FetchStorageInfo()
        {
            return Task.FromResult(new StorageInfo());
        }

        public Task<CameraStatus> FetchCameraStatus()
        {
            return Task.FromResult(new CameraStatus());
        }

        private static async Task Map(Task task)
        {
            try
            {
                await task;
            }
            catch (GPhoto2Exception ex)
            {
                throw MapException(ex);
            }
        }

        /// <summary>
        /// Maps libgphoto2 errors to the photo booth exceptions (localized error messages in the client).
        /// </summary>
        public static Exception MapException(GPhoto2Exception ex)
        {
            string message = ex.Message ?? string.Empty;

            if (message.Contains("Out of Focus", StringComparison.OrdinalIgnoreCase))
            {
                return new CameraOutOfFocusException("Camera Out of Focus");
            }

            if (message.Contains("PTP Store Not Available", StringComparison.OrdinalIgnoreCase))
            {
                return new PtpStoreException();
            }

            switch (ex.Code)
            {
                case GPhoto2Exception.ErrorIoUsbClaim:
                    return new CameraClaimException("Failed to claim camera");
                case GPhoto2Exception.ErrorModelNotFound:
                case GPhoto2Exception.ErrorIoUsbFind:
                case GPhoto2Exception.ErrorUnknownPort:
                    return new CameraNotAvailableException("No camera found");
                default:
                    return new CameraException(message);
            }
        }
    }
}
