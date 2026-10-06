using System;
using System.Threading;
using System.Threading.Tasks;

namespace PhotoBooth.Abstraction.LiveView
{
    public interface ILiveViewService
    {
        event EventHandler StatusChanged;

        bool IsEnabled { get; }

        bool IsRunning { get; }

        /// <summary>
        /// See <see cref="ILiveViewSource.SupportsCaptureDuringLiveView"/>.
        /// </summary>
        bool SupportsCaptureDuringLiveView { get; }

        LiveViewStatus GetStatus();

        /// <summary>
        /// Starts the live view if it is enabled and not running yet.
        /// </summary>
        /// <param name="canStart">Optional condition, evaluated while holding the start/stop lock.</param>
        Task StartAsync(Func<bool> canStart = null);

        /// <summary>
        /// Stops the live view and waits until the camera is released.
        /// </summary>
        Task StopAsync();

        /// <summary>
        /// Returns the next frame newer than <paramref name="afterFrameId"/>,
        /// or null if the live view is not running (anymore).
        /// </summary>
        Task<LiveViewFrame> WaitForFrameAsync(long afterFrameId, CancellationToken token);

        /// <summary>
        /// Registers a connected stream client (for the status), dispose to unregister.
        /// </summary>
        IDisposable RegisterViewer();
    }
}
