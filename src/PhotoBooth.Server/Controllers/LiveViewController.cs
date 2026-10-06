using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PhotoBooth.Abstraction;
using PhotoBooth.Abstraction.LiveView;

namespace PhotoBooth.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class LiveViewController : ControllerBase
    {
        private const string Boundary = "frame";

        private readonly ILogger<LiveViewController> _logger;
        private readonly ILiveViewService _liveViewService;
        private readonly IWorkflowController _workflowController;

        public LiveViewController(ILogger<LiveViewController> logger, ILiveViewService liveViewService, IWorkflowController workflowController)
        {
            _logger = logger;
            _liveViewService = liveViewService;
            _workflowController = workflowController;
        }

        [HttpGet]
        [ActionName(nameof(Status))]
        public LiveViewStatus Status()
        {
            return _liveViewService.GetStatus();
        }

        /// <summary>
        /// (Re)starts the live view, only allowed while the photo booth is ready or counting down
        /// (the camera is used by the capture otherwise).
        /// </summary>
        [HttpPost]
        [ActionName(nameof(Start))]
        public async Task<IActionResult> Start()
        {
            if (!_liveViewService.IsEnabled)
            {
                return Conflict("Live view is disabled");
            }

            if (!IsLiveViewAllowed())
            {
                return Conflict($"Live view not possible in state {_workflowController.State}");
            }

            await _liveViewService.StartAsync(IsLiveViewAllowed);
            return Ok(_liveViewService.GetStatus());
        }

        [HttpPost]
        [ActionName(nameof(Stop))]
        public async Task<LiveViewStatus> Stop()
        {
            await _liveViewService.StopAsync();
            return _liveViewService.GetStatus();
        }

        /// <summary>
        /// MJPEG stream (multipart/x-mixed-replace), can be used directly as image source:
        /// &lt;img src="api/LiveView/Stream"&gt;. The stream ends when the live view stops.
        /// </summary>
        [HttpGet]
        [ActionName(nameof(Stream))]
        public async Task Stream(CancellationToken token)
        {
            Response.ContentType = $"multipart/x-mixed-replace; boundary={Boundary}";
            Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
            HttpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();

            using (_liveViewService.RegisterViewer())
            {
                long lastFrameId = 0;

                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        LiveViewFrame frame = await _liveViewService.WaitForFrameAsync(lastFrameId, token);

                        if (frame == null)
                        {
                            // live view not running (anymore)
                            break;
                        }

                        lastFrameId = frame.Id;

                        byte[] header = Encoding.ASCII.GetBytes($"--{Boundary}\r\nContent-Type: image/jpeg\r\nContent-Length: {frame.Data.Length}\r\n\r\n");
                        await Response.Body.WriteAsync(header, token);
                        await Response.Body.WriteAsync(frame.Data, token);
                        await Response.Body.WriteAsync(Encoding.ASCII.GetBytes("\r\n"), token);
                        await Response.Body.FlushAsync(token);
                    }
                }
                catch (OperationCanceledException)
                {
                    // client disconnected
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Live view stream closed");
                }
            }
        }

        private bool IsLiveViewAllowed()
        {
            CaptureProcessState state = _workflowController.State;
            return state == CaptureProcessState.Ready || state == CaptureProcessState.CountDown;
        }
    }
}
