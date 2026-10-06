using System;
using System.Threading;
using System.Threading.Tasks;
using PhotoBooth.Abstraction.LiveView;

namespace PhotoBooth.Service.LiveView
{
    /// <summary>
    /// Disabled live view.
    /// </summary>
    public class NullLiveViewService : ILiveViewService
    {
        public event EventHandler StatusChanged
        {
            add { }
            remove { }
        }

        public bool IsEnabled
        {
            get
            {
                return false;
            }
        }

        public bool IsRunning
        {
            get
            {
                return false;
            }
        }

        public LiveViewStatus GetStatus()
        {
            return new LiveViewStatus();
        }

        public Task StartAsync(Func<bool> canStart = null)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            return Task.CompletedTask;
        }

        public Task<LiveViewFrame> WaitForFrameAsync(long afterFrameId, CancellationToken token)
        {
            return Task.FromResult<LiveViewFrame>(null);
        }

        public IDisposable RegisterViewer()
        {
            return new Viewer();
        }

        private sealed class Viewer : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
