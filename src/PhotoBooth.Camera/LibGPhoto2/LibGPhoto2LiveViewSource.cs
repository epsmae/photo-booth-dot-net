using System;
using System.Threading;
using System.Threading.Tasks;
using PhotoBooth.Abstraction.LiveView;

namespace PhotoBooth.Camera.LibGPhoto2
{
    /// <summary>
    /// Live view frames from the shared libgphoto2 camera session. Captures can run while the live
    /// view is active, the session executes them between two preview frames.
    /// </summary>
    public class LibGPhoto2LiveViewSource : ILiveViewSource
    {
        private readonly CameraSession _session;

        public LibGPhoto2LiveViewSource(CameraSession session)
        {
            _session = session;
        }

        public bool SupportsCaptureDuringLiveView
        {
            get
            {
                return true;
            }
        }

        public async Task RunAsync(Action<byte[]> onFrame, CancellationToken stopToken, CancellationToken killToken)
        {
            TaskCompletionSource<bool> completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            _session.StartPreview(onFrame, ex => completion.TrySetException(new LiveViewException(ex.Message)));

            try
            {
                using (stopToken.Register(() => completion.TrySetCanceled(stopToken)))
                using (killToken.Register(() => completion.TrySetCanceled(killToken)))
                {
                    await completion.Task.ConfigureAwait(false);
                }
            }
            finally
            {
                // only stops fetching frames, the camera stays in live view for a fast restart
                _session.StopPreview();
            }
        }
    }
}
