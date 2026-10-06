using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PhotoBooth.Camera.LibGPhoto2
{
    /// <summary>
    /// P/Invoke declarations for libgphoto2 (verified against the libgphoto2 2.5 headers).
    /// </summary>
    internal static class GPhoto2Native
    {
        private const string LibraryName = "gphoto2";

        // runtime package (libgphoto2-6) only contains the versioned name, the -dev package adds libgphoto2.so
        private static readonly string[] LibraryCandidates = {"libgphoto2.so.6", "libgphoto2.so", "libgphoto2.6.dylib", "libgphoto2.dylib"};

        private static readonly object ResolverLock = new object();
        private static bool _resolverRegistered;

        public const int GP_OK = 0;
        public const int GP_CAPTURE_IMAGE = 0;
        public const int GP_FILE_TYPE_NORMAL = 1;
        public const int GP_WIDGET_TEXT = 2;
        public const int GP_WIDGET_RANGE = 3;
        public const int GP_WIDGET_TOGGLE = 4;
        public const int GP_WIDGET_RADIO = 5;
        public const int GP_WIDGET_MENU = 6;

        // sizeof(CameraAbilities) is 2504 bytes, the model name is the first member (char[128])
        public const int CameraAbilitiesBufferSize = 4096;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct CameraFilePath
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string name;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
            public string folder;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void GPContextErrorFunc(IntPtr context, IntPtr text, IntPtr data);

        public static void EnsureLibraryResolver()
        {
            lock (ResolverLock)
            {
                if (_resolverRegistered)
                {
                    return;
                }

                NativeLibrary.SetDllImportResolver(typeof(GPhoto2Native).Assembly, Resolve);
                _resolverRegistered = true;
            }
        }

        private static IntPtr Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (libraryName != LibraryName)
            {
                return IntPtr.Zero;
            }

            foreach (string candidate in LibraryCandidates)
            {
                if (NativeLibrary.TryLoad(candidate, assembly, searchPath, out IntPtr handle))
                {
                    return handle;
                }
            }

            throw new DllNotFoundException($"libgphoto2 not found (tried {string.Join(", ", LibraryCandidates)}), install it with: sudo apt-get install libgphoto2-6");
        }

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr gp_context_new();

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void gp_context_unref(IntPtr context);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void gp_context_set_error_func(IntPtr context, GPContextErrorFunc func, IntPtr data);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr gp_result_as_string(int result);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gp_camera_new(out IntPtr camera);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gp_camera_init(IntPtr camera, IntPtr context);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gp_camera_exit(IntPtr camera, IntPtr context);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gp_camera_unref(IntPtr camera);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gp_camera_get_abilities(IntPtr camera, IntPtr abilities);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gp_camera_capture_preview(IntPtr camera, IntPtr file, IntPtr context);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gp_camera_capture(IntPtr camera, int type, ref CameraFilePath path, IntPtr context);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gp_camera_file_get(IntPtr camera, [MarshalAs(UnmanagedType.LPUTF8Str)] string folder,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string file, int type, IntPtr cameraFile, IntPtr context);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gp_camera_get_single_config(IntPtr camera, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, out IntPtr widget, IntPtr context);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gp_camera_set_single_config(IntPtr camera, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, IntPtr widget, IntPtr context);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gp_file_new(out IntPtr file);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gp_file_unref(IntPtr file);

        // unsigned long int *size: pointer sized on Linux (LP64 / ILP32)
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gp_file_get_data_and_size(IntPtr file, out IntPtr data, out nuint size);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gp_file_save(IntPtr file, [MarshalAs(UnmanagedType.LPUTF8Str)] string filename);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gp_widget_free(IntPtr widget);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gp_widget_get_type(IntPtr widget, out int type);

        // text, radio, menu: const char*
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gp_widget_set_value(IntPtr widget, [MarshalAs(UnmanagedType.LPUTF8Str)] string value);

        // toggle: int*
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gp_widget_set_value(IntPtr widget, ref int value);

        // range: float*
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gp_widget_set_value(IntPtr widget, ref float value);
    }
}
