using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using PhotoBooth.Abstraction;

namespace PhotoBooth.Server
{
    public class CaptureHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients?.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task SendStateChanged(CaptureProcessState state)
        {
            if (Clients != null && Clients.All  != null)
            {
                await Clients?.All.SendAsync("ReceiveStateChanged", state);
            }
        }

        public async Task SendCountDownStepChanged(int step)
        {
            if (Clients != null && Clients.All != null)
            {
                await Clients?.All.SendAsync("ReceiveCountDownStepChanged", step);
            }
        }

        public async Task SendReviewCountDownStepChanged(int step)
        {
            if (Clients != null && Clients.All != null)
            {
                await Clients?.All.SendAsync("ReceiveReviewCountDownStepChanged", step);
            }
        }
    }
}
