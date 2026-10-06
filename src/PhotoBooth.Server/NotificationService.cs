using System;
using System.Threading.Tasks;
using PhotoBooth.Abstraction;
using PhotoBooth.Abstraction.LiveView;

namespace PhotoBooth.Server
{
    public class NotificationService
    {
        private readonly IWorkflowController _workflowController;
        private readonly CaptureHub _hub;
        private readonly ILiveViewService _liveViewService;

        public NotificationService(IWorkflowController workflowController, CaptureHub hub, ILiveViewService liveViewService)
        {
            _workflowController = workflowController;
            _hub = hub;
            _liveViewService = liveViewService;
            _liveViewService.StatusChanged += OnLiveViewStatusChanged;
            _workflowController.StateChanged += OnStateChanged;
            _workflowController.CountDownChanged += OnCountDownStepChanged;
            _workflowController.ReviewCountDownChanged += OnReviewCountDownChanged;
        }

        private async void OnReviewCountDownChanged(object? sender, EventArgs e)
        {
            await _hub.SendReviewCountDownStepChanged(_workflowController.CurrentReviewCountDown);
        }

        private async void OnCountDownStepChanged(object? sender, EventArgs e)
        {
            await _hub.SendCountDownStepChanged(_workflowController.CurrentCountDownStep);
        }

        private async void OnLiveViewStatusChanged(object sender, EventArgs e)
        {
            try
            {
                await _hub.SendLiveViewStatusChanged(_liveViewService.GetStatus());
            }
            catch
            {
                // ignore, clients poll the status as fallback
            }
        }

        private async void OnStateChanged(object? sender, EventArgs e)
        {
            await SendStateUpdate();
        }

        public Task SendStateUpdate()
        {
            return _hub.SendStateChanged(new CaptureState
            {
                ProcessState = _workflowController.State,
                RequiredImageCount = _workflowController.RequiredImageCount,
                CaptureLayout = _workflowController.ActiveCaptureLayout,
                CurrentImageIndex = _workflowController.CurrentImageIndex,
                PrinterQueueCount = _workflowController.PrinterQueueCount,
                PrinterName = _workflowController.PrinterName
            });
        }
    }
}
