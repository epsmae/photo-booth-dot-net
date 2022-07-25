namespace PhotoBooth.Abstraction
{
    public class ImageOffsetInfo
    {

        public ImageOffsetInfo(double width, double height, double topOffset, double leftOffset)
        {
            Width = width;
            Height = height;
            TopOffset = topOffset;
            LeftOffset = leftOffset;
        }

        public double Width
        {
            get;
            set;
        }

        public double Height
        {
            get;
            set;
        }

        public double TopOffset
        {
            get;
            set;
        }

        public double LeftOffset
        {
            get;
            set;
        }
    }
}
