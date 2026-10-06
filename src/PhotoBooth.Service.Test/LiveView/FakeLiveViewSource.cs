using System;
using System.Threading;
using System.Threading.Tasks;
using PhotoBooth.Abstraction.LiveView;

namespace PhotoBooth.Service.Test.LiveView
{
    internal class FakeLiveViewSource : ILiveViewSource
    {
        private int _runCount;

        public bool IgnoreStop { get; set; }

        public bool SupportsCaptureDuringLiveView { get; set; }

        public Exception FailWith { get; set; }

        public int RunCount
        {
            get
            {
                return Volatile.Read(ref _runCount);
            }
        }

        public async Task RunAsync(Action<byte[]> onFrame, CancellationToken stopToken, CancellationToken killToken)
        {
            Interlocked.Increment(ref _runCount);
            CancellationToken token = IgnoreStop ? killToken : CancellationTokenSource.CreateLinkedTokenSource(stopToken, killToken).Token;

            for (int i = 0; ; i++)
            {
                if (FailWith != null && i == 3)
                {
                    throw FailWith;
                }

                onFrame(new byte[] {0xFF, 0xD8, (byte) i, 0xFF, 0xD9});
                await Task.Delay(10, token);
            }
        }
    }
}
