using System;
using System.Globalization;
using System.Runtime.InteropServices;
using PhotoBooth.Abstraction.LibGPhoto2;

namespace PhotoBooth.Camera.LibGPhoto2
{
    /// <summary>
    /// libgphoto2 camera access via P/Invoke. Not thread safe, use it from one thread only.
    /// </summary>
    public sealed class GPhoto2Api : IGPhoto2Api
    {
        private readonly IntPtr _context;

        // keep the delegate alive as long as the context exists
        private readonly GPhoto2Native.GPContextErrorFunc _errorFunc;

        private IntPtr _camera;
        private string _lastContextError;
        private bool _disposed;

        public GPhoto2Api()
        {
            GPhoto2Native.EnsureLibraryResolver();

            _context = GPhoto2Native.gp_context_new();

            if (_context == IntPtr.Zero)
            {
                throw new GPhoto2Exception(GPhoto2Exception.ErrorGeneric, "gp_context_new failed");
            }

            _errorFunc = OnContextError;
            GPhoto2Native.gp_context_set_error_func(_context, _errorFunc, IntPtr.Zero);
        }

        public bool IsConnected
        {
            get
            {
                return _camera != IntPtr.Zero;
            }
        }

        public void Connect()
        {
            ThrowIfDisposed();

            if (IsConnected)
            {
                return;
            }

            Check(GPhoto2Native.gp_camera_new(out IntPtr camera), "gp_camera_new");

            _lastContextError = null;
            int result = GPhoto2Native.gp_camera_init(camera, _context);

            if (result < GPhoto2Native.GP_OK)
            {
                GPhoto2Native.gp_camera_unref(camera);
                throw CreateException(result, "gp_camera_init");
            }

            _camera = camera;
        }

        public void Disconnect()
        {
            if (!IsConnected)
            {
                return;
            }

            // ends the live view and the remote control mode on the camera
            GPhoto2Native.gp_camera_exit(_camera, _context);
            GPhoto2Native.gp_camera_unref(_camera);
            _camera = IntPtr.Zero;
        }

        public string GetModel()
        {
            ThrowIfNotConnected();

            IntPtr abilities = Marshal.AllocHGlobal(GPhoto2Native.CameraAbilitiesBufferSize);

            try
            {
                Check(GPhoto2Native.gp_camera_get_abilities(_camera, abilities), "gp_camera_get_abilities");
                return Marshal.PtrToStringUTF8(abilities);
            }
            finally
            {
                Marshal.FreeHGlobal(abilities);
            }
        }

        public byte[] CapturePreview()
        {
            ThrowIfNotConnected();

            Check(GPhoto2Native.gp_file_new(out IntPtr file), "gp_file_new");

            try
            {
                Check(GPhoto2Native.gp_camera_capture_preview(_camera, file, _context), "gp_camera_capture_preview");
                return GetFileData(file);
            }
            finally
            {
                GPhoto2Native.gp_file_unref(file);
            }
        }

        public CameraFileLocation CaptureImage()
        {
            ThrowIfNotConnected();

            GPhoto2Native.CameraFilePath path = new GPhoto2Native.CameraFilePath();
            Check(GPhoto2Native.gp_camera_capture(_camera, GPhoto2Native.GP_CAPTURE_IMAGE, ref path, _context), "gp_camera_capture");

            return new CameraFileLocation(path.folder, path.name);
        }

        public void DownloadFile(CameraFileLocation location, string targetFile)
        {
            ThrowIfNotConnected();

            Check(GPhoto2Native.gp_file_new(out IntPtr file), "gp_file_new");

            try
            {
                Check(GPhoto2Native.gp_camera_file_get(_camera, location.Folder, location.Name, GPhoto2Native.GP_FILE_TYPE_NORMAL, file, _context), "gp_camera_file_get");
                Check(GPhoto2Native.gp_file_save(file, targetFile), "gp_file_save");
            }
            finally
            {
                GPhoto2Native.gp_file_unref(file);
            }
        }

        public void SetConfig(string name, string value)
        {
            ThrowIfNotConnected();

            Check(GPhoto2Native.gp_camera_get_single_config(_camera, name, out IntPtr widget, _context), $"gp_camera_get_single_config({name})");

            try
            {
                Check(GPhoto2Native.gp_widget_get_type(widget, out int type), "gp_widget_get_type");

                switch (type)
                {
                    case GPhoto2Native.GP_WIDGET_TOGGLE:
                        int toggle = int.Parse(value, CultureInfo.InvariantCulture);
                        Check(GPhoto2Native.gp_widget_set_value(widget, ref toggle), "gp_widget_set_value");
                        break;
                    case GPhoto2Native.GP_WIDGET_RANGE:
                        float range = float.Parse(value, CultureInfo.InvariantCulture);
                        Check(GPhoto2Native.gp_widget_set_value(widget, ref range), "gp_widget_set_value");
                        break;
                    case GPhoto2Native.GP_WIDGET_TEXT:
                    case GPhoto2Native.GP_WIDGET_RADIO:
                    case GPhoto2Native.GP_WIDGET_MENU:
                        Check(GPhoto2Native.gp_widget_set_value(widget, value), "gp_widget_set_value");
                        break;
                    default:
                        throw new GPhoto2Exception(GPhoto2Exception.ErrorNotSupported, $"Config {name} has unsupported widget type {type}");
                }

                Check(GPhoto2Native.gp_camera_set_single_config(_camera, name, widget, _context), $"gp_camera_set_single_config({name})");
            }
            finally
            {
                GPhoto2Native.gp_widget_free(widget);
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Disconnect();
            GPhoto2Native.gp_context_unref(_context);
            _disposed = true;
        }

        private byte[] GetFileData(IntPtr file)
        {
            Check(GPhoto2Native.gp_file_get_data_and_size(file, out IntPtr data, out nuint size), "gp_file_get_data_and_size");

            byte[] buffer = new byte[checked((int) size)];
            Marshal.Copy(data, buffer, 0, buffer.Length);
            return buffer;
        }

        private void Check(int result, string operation)
        {
            if (result < GPhoto2Native.GP_OK)
            {
                throw CreateException(result, operation);
            }

            _lastContextError = null;
        }

        private GPhoto2Exception CreateException(int result, string operation)
        {
            // the context error contains the camera specific reason, e.g. "Out of Focus"
            string contextError = _lastContextError;
            _lastContextError = null;
            string detail = string.IsNullOrEmpty(contextError) ? string.Empty : $": {contextError.Trim()}";

            return new GPhoto2Exception(result, $"{operation} failed: {ResultAsString(result)} ({result}){detail}");
        }

        private static string ResultAsString(int result)
        {
            // static string owned by libgphoto2, must not be freed
            return Marshal.PtrToStringUTF8(GPhoto2Native.gp_result_as_string(result)) ?? "unknown error";
        }

        private void OnContextError(IntPtr context, IntPtr text, IntPtr data)
        {
            _lastContextError = Marshal.PtrToStringUTF8(text);
        }

        private void ThrowIfNotConnected()
        {
            ThrowIfDisposed();

            if (!IsConnected)
            {
                throw new GPhoto2Exception(GPhoto2Exception.ErrorModelNotFound, "Camera not connected");
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(GPhoto2Api));
            }
        }
    }
}
