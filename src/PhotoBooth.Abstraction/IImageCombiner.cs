namespace PhotoBooth.Abstraction
{
    public interface IImageCombiner
    {
        void Combine(string image1SrcPath, string image2SrcPath, string destinationPath);
    }
}
