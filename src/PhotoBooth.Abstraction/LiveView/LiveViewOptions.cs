namespace PhotoBooth.Abstraction.LiveView
{
    /// <summary>
    /// Configuration section "LiveView" in appsettings.json.
    /// </summary>
    public class LiveViewOptions
    {
        public const string SectionName = "LiveView";

        public const string SourceGPhoto2 = "GPhoto2";

        public const string SourceSimulator = "Simulator";

        /// <summary>
        /// "GPhoto2" or "Simulator", empty: simulator in debug builds, gphoto2 in release builds.
        /// </summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// Enables the live view on the capture page.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Mirror the live view horizontally (selfie view).
        /// </summary>
        public bool Mirror { get; set; } = true;

        /// <summary>
        /// Stops the live view after this many seconds without a new capture/start (protects the
        /// camera from overheating and saves battery). 0 disables the timeout.
        /// </summary>
        public int IdleTimeoutSeconds { get; set; } = 300;

        /// <summary>
        /// How long to wait for gphoto2 to exit after SIGINT before the process is killed.
        /// </summary>
        public int StopTimeoutMilliseconds { get; set; } = 5000;

        /// <summary>
        /// Optional camera model passed to gphoto2 --camera (empty: first detected camera).
        /// </summary>
        public string Camera { get; set; } = string.Empty;

        /// <summary>
        /// gphoto2 executable, e.g. tools/fake-gphoto2/gphoto2 for development without a camera.
        /// </summary>
        public string GPhoto2Path { get; set; } = "gphoto2";
    }
}
