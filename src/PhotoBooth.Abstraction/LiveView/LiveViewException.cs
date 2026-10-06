using System;

namespace PhotoBooth.Abstraction.LiveView
{
    public class LiveViewException : Exception
    {
        public LiveViewException(string message) : base(message)
        {
        }
    }
}
