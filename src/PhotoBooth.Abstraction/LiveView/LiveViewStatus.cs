namespace PhotoBooth.Abstraction.LiveView
{
    public class LiveViewStatus
    {
        public bool Enabled { get; set; }

        public bool Running { get; set; }

        public bool Mirror { get; set; }

        public long FrameCount { get; set; }

        public double FramesPerSecond { get; set; }

        public int Viewers { get; set; }

        public string LastError { get; set; }
    }
}
