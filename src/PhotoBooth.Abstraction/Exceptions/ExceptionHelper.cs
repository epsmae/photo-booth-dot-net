using System;

namespace PhotoBooth.Abstraction.Exceptions
{
    public static class ExceptionHelper
    {
        public static PhotoBoothExceptions Convert(Exception ex)
        {
            if (ex is CameraException)
            {
                if (ex is PtpStoreException || ex is CameraFileNotFoundException)
                {
                    return PhotoBoothExceptions.CameraSdCardIssue;
                }

                if (ex is CameraNotAvailableException)
                {
                    return PhotoBoothExceptions.NoCameraAvailable;
                }

                if (ex is CameraOutOfFocusException)
                {
                    return PhotoBoothExceptions.CameraOutOfFocus;
                }

                if (ex is CameraClaimException)
                {
                    return PhotoBoothExceptions.CameraClaimFailed;
                }

                return PhotoBoothExceptions.UnknownError;

            }

            if (ex is PrinterException)
            {
                if (ex is PrinterNotAvailableException)
                {
                    return PhotoBoothExceptions.NoPrinterAvailable;
                }

                return PhotoBoothExceptions.GeneralPrinterError;
            }

            return PhotoBoothExceptions.UnknownError;
        }
    }
}
