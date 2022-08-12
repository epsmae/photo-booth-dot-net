namespace PhotoBooth.Abstraction
{
    public class CaptureState
    {
        public CaptureProcessState ProcessState { get; set; }

        public int CurrentImageIndex { get; set; }

        public CaptureLayouts CaptureLayout { get; set; }

        public int RequiredImageCount
        {
            get;
            set;
        }

    }
}
