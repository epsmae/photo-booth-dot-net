namespace PhotoBooth.Abstraction.LibGPhoto2
{
    public enum CameraDriver
    {
        /// <summary>
        /// gphoto2 command line tool, one process per action (live view needs to be stopped for a capture).
        /// </summary>
        Cli,

        /// <summary>
        /// libgphoto2 in process, one camera connection for live view and capture.
        /// </summary>
        LibGPhoto2
    }

    public enum CameraFocusMode
    {
        /// <summary>
        /// Capture directly out of the live view without focusing (manual focus / pre-focused lens).
        /// Fastest, the live view continues right after the capture.
        /// </summary>
        None,

        /// <summary>
        /// Contrast autofocus in live view (Nikon "autofocusdrive") right before the capture.
        /// </summary>
        LiveViewAutofocus,

        /// <summary>
        /// Leave the live view (mirror down) and capture with the regular phase detection autofocus.
        /// The live view restarts with the next preview frame (about 1-2 s, no process restart).
        /// </summary>
        PhaseDetect
    }

    /// <summary>
    /// Configuration section "Camera" in appsettings.json.
    /// </summary>
    public class CameraDriverOptions
    {
        public const string SectionName = "Camera";

        public CameraDriver Driver { get; set; } = CameraDriver.LibGPhoto2;

        /// <summary>
        /// Use a simulated camera, default: true in debug builds, false in release builds.
        /// </summary>
        public bool? Simulate { get; set; }

        public CameraFocusMode FocusMode { get; set; } = CameraFocusMode.None;

        /// <summary>
        /// Value for the camera config "capturetarget" (Nikon: "Memory card" or "Internal RAM"),
        /// empty: do not change.
        /// </summary>
        public string CaptureTarget { get; set; } = "Memory card";

        /// <summary>
        /// Keep the camera in live view for this many seconds after the last preview frame (fast
        /// restart e.g. after the review), then end the live view (mirror down).
        /// </summary>
        public int LiveViewHoldSeconds { get; set; } = 60;

        /// <summary>
        /// Consecutive preview errors before the live view is reported as failed.
        /// </summary>
        public int MaxPreviewErrors { get; set; } = 5;
    }
}
