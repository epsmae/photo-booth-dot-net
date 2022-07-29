namespace PhotoBooth.Abstraction
{
    public interface IImageGalleryOffsetCalculator
    {
        int RequiredImageCount { get; }

        ImageOffsetInfo GetOffset(int index, int maxWidth, int maxHeight);
    }
}
