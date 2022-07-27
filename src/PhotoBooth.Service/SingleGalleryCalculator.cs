using PhotoBooth.Abstraction;

namespace PhotoBooth.Service
{
    public class SingleGalleryCalculator : IImageGalleryOffsetCalculator
    {
        public int RequiredImageCount
        {
            get
            {
                return 1;
            }
        }

        public ImageOffsetInfo GetOffset(int index, int maxWidth, int maxHeight)
        {
            return new ImageOffsetInfo(0, 0, maxWidth, maxHeight);
        }
    }
}
