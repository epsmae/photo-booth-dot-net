using System;
using PhotoBooth.Abstraction;

namespace PhotoBooth.Service
{
    public class FourImageGalleryCalculator : IImageGalleryOffsetCalculator
    {
        public int RequiredImageCount
        {
            get
            {
                return 4;
            }
        }

        public ImageOffsetInfo GetOffset(int index, int maxWidth, int maxHeight)
        {
            const double widthSpacingPercentage = 0.05;

            double totalSpacingWidth = widthSpacingPercentage * maxWidth;
            double singleSpacingWidth = totalSpacingWidth / 3.0;

            double newWidth = (maxWidth - totalSpacingWidth) / 2.0;
            double scaling = newWidth / maxWidth;

            double newHeight = scaling * maxHeight;

            double totalSpacingHeight = maxHeight - 2 * newHeight;
            double singleSpacingHeight = totalSpacingHeight / 3.0;

            switch (index)
            {
                case 0:
                    return new ImageOffsetInfo(newWidth, newHeight, singleSpacingHeight, singleSpacingWidth);
                case 1:
                    return new ImageOffsetInfo(newWidth, newHeight, 2 * singleSpacingHeight + newHeight, singleSpacingWidth);
                case 2:
                    return new ImageOffsetInfo(newWidth, newHeight, singleSpacingHeight, 2 * singleSpacingWidth + newWidth);
                case 3:
                    return new ImageOffsetInfo(newWidth, newHeight, 2 * singleSpacingHeight + newHeight, 2 * singleSpacingWidth + newWidth);

                default:
                    throw new IndexOutOfRangeException("Only 0 to 3 supported");
            }
        }


    }
}
