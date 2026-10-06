using System;
using System.Threading;
using System.Threading.Tasks;

namespace PhotoBooth.Abstraction.LiveView
{
    /// <summary>
    /// Produces live view (preview) frames as JPEG images.
    /// </summary>
    public interface ILiveViewSource
    {
        /// <summary>
        /// True if the camera can capture while this live view is running (shared camera connection),
        /// false if the live view has to be stopped (camera released) before a capture.
        /// </summary>
        bool SupportsCaptureDuringLiveView { get; }

        /// <summary>
        /// Runs the live view until it ends by itself or one of the tokens is cancelled.
        /// </summary>
        /// <param name="onFrame">Called for every received JPEG frame.</param>
        /// <param name="stopToken">Requests a graceful stop (e.g. SIGINT to gphoto2 so the camera leaves live view).</param>
        /// <param name="killToken">Forcefully terminates the live view.</param>
        Task RunAsync(Action<byte[]> onFrame, CancellationToken stopToken, CancellationToken killToken);
    }
}
