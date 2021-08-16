using System;

namespace PhotoBooth.Abstraction.Exceptions
{
    public static class ExceptionHelper
    {

        public static PhotoBoothExceptions Convert(Exception ex)
        {
            if (ex is CameraException)
            {
                if (ex is NoCameraAvailableException)
                {
                    return PhotoBoothExceptions.NoCameraAvailable;
                }

                if (ex is OutOfFocusException)
                {
                    return PhotoBoothExceptions.CameraOutOfFocus;
                }

                return PhotoBoothExceptions.UnknownError;

            }

            if (ex is PrinterException)
            {
                if (ex is NoPrinterAvailableException)
                {
                    return PhotoBoothExceptions.NoPrinterAvailable;
                }

                return PhotoBoothExceptions.GeneralPrinterError;
            }

            return PhotoBoothExceptions.UnknownError;

        }
    }
}
