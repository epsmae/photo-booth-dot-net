using System;

namespace PhotoBooth.Abstraction.LibGPhoto2
{
    /// <summary>
    /// Thin wrapper around the libgphoto2 camera functions used by the photo booth.
    /// Not thread safe, all calls have to be made from the same thread (see CameraSession).
    /// </summary>
    public interface IGPhoto2Api : IDisposable
    {
        bool IsConnected { get; }

        /// <summary>
        /// gp_camera_new + gp_camera_init (first detected camera).
        /// </summary>
        void Connect();

        /// <summary>
        /// gp_camera_exit + gp_camera_unref, ends the live view on the camera.
        /// </summary>
        void Disconnect();

        string GetModel();

        /// <summary>
        /// gp_camera_capture_preview, starts the live view on the first call. Returns a JPEG frame.
        /// </summary>
        byte[] CapturePreview();

        /// <summary>
        /// gp_camera_capture(GP_CAPTURE_IMAGE), works while the live view is active.
        /// </summary>
        CameraFileLocation CaptureImage();

        /// <summary>
        /// gp_camera_file_get + gp_file_save.
        /// </summary>
        void DownloadFile(CameraFileLocation location, string targetFile);

        /// <summary>
        /// Sets a camera configuration value (gp_camera_get/set_single_config). The value is
        /// converted to the widget type (text/radio: choice name, toggle: 0/1, range: number).
        /// </summary>
        void SetConfig(string name, string value);
    }
}
