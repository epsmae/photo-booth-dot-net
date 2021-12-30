using System;
using Microsoft.Extensions.Logging;
using PhotoBooth.Abstraction;

namespace PhotoBooth.Service
{
    public class HardwareController : IHardwareController
    {
        private readonly ILogger<HardwareController> _logger;
        private readonly IWorkflowController _workflowController;
        private readonly IGpioInterface _gpioInterface;

        public HardwareController(ILogger<HardwareController> logger, IWorkflowController workflowController, IGpioInterface gpioInterface)
        {
            _logger = logger;
            _workflowController = workflowController;
            _gpioInterface = gpioInterface;

            _workflowController.StateChanged += OnStateChanged;
            _gpioInterface.PrimaryButtonChanged += OnPrimaryButtonChanged;
        }

        public void Initialize()
        {
            SetPrimaryButtonBlinkState();
        }


        private async void OnPrimaryButtonChanged(object sender, bool value)
        {
            try
            {
                if (value)
                {
                    await _workflowController.Capture();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to capture");
            }
        }

        private void OnStateChanged(object sender, EventArgs e)
        {
            SetPrimaryButtonBlinkState();
        }

        private void SetPrimaryButtonBlinkState()
        {
            if (_workflowController.State == CaptureProcessState.Ready)
            {
                _gpioInterface.StartBlinkingPrimaryButton();
            }
            else
            {
                _gpioInterface.StopBlinkingPrimaryButton();
            }
        }
    }
}
