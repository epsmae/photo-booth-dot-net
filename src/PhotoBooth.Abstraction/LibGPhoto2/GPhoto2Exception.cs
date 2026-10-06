using System;

namespace PhotoBooth.Abstraction.LibGPhoto2
{
    public class GPhoto2Exception : Exception
    {
        public const int ErrorGeneric = -1;
        public const int ErrorUnknownPort = -5;
        public const int ErrorNotSupported = -6;
        public const int ErrorIo = -7;
        public const int ErrorTimeout = -10;
        public const int ErrorIoInit = -31;
        public const int ErrorIoRead = -34;
        public const int ErrorIoWrite = -35;
        public const int ErrorIoUpdate = -37;
        public const int ErrorIoUsbClearHalt = -51;
        public const int ErrorIoUsbFind = -52;
        public const int ErrorIoUsbClaim = -53;
        public const int ErrorIoLock = -60;
        public const int ErrorModelNotFound = -105;
        public const int ErrorCameraBusy = -110;

        public GPhoto2Exception(int code, string message) : base(message)
        {
            Code = code;
        }

        public int Code { get; }

        public bool IsBusy
        {
            get
            {
                return Code == ErrorCameraBusy;
            }
        }

        /// <summary>
        /// The camera connection is broken (unplugged, switched off, claimed by another process),
        /// the camera has to be reconnected.
        /// </summary>
        public bool IsConnectionError
        {
            get
            {
                switch (Code)
                {
                    case ErrorUnknownPort:
                    case ErrorIo:
                    case ErrorTimeout:
                    case ErrorIoInit:
                    case ErrorIoRead:
                    case ErrorIoWrite:
                    case ErrorIoUpdate:
                    case ErrorIoUsbClearHalt:
                    case ErrorIoUsbFind:
                    case ErrorIoUsbClaim:
                    case ErrorIoLock:
                    case ErrorModelNotFound:
                        return true;
                    default:
                        return false;
                }
            }
        }
    }
}
