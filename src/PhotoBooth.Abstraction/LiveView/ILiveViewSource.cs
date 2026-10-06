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
        /// Runs the live view until it ends by itself or one of the tokens is cancelled.
        /// </summary>
        /// <param name="onFrame">Called for every received JPEG frame.</param>
        /// <param name="stopToken">Requests a graceful stop (e.g. SIGINT to gphoto2 so the camera leaves live view).</param>
        /// <param name="killToken">Forcefully terminates the live view.</param>
        Task RunAsync(Action<byte[]> onFrame, CancellationToken stopToken, CancellationToken killToken);
    }
}
