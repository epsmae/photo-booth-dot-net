namespace PhotoBooth.Abstraction
{
    public class PreviewCaptureResult
    {
        public string FileName { get; set; }

        public byte[] ThumbnailData
        {
            get; set;
        }
    }
}
