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
            _gpioInterface.SecondaryButtonChanged += OnSecondaryButtonChanged;
        }

        public void Initialize()
        {
            SetPrimaryButtonBlinkState();
        }

        private async void OnSecondaryButtonChanged(object sender, bool value)
        {
            try
            {
                if (value)
                {
                    if (_workflowController.State == CaptureProcessState.Review)
                    {
                        await _workflowController.Skip();
                    }
                    else if (_workflowController.State == CaptureProcessState.Error)
                    {
                        await _workflowController.ConfirmError();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle secondary button");
            }
        }


        private async void OnPrimaryButtonChanged(object sender, bool value)
        {
            try
            {
                if (value)
                {
                    if (_workflowController.State == CaptureProcessState.Ready)
                    {
                        await _workflowController.Capture();
                    }
                    else if (_workflowController.State == CaptureProcessState.Error)
                    {
                        await _workflowController.ConfirmError();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle primary button");
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

            if (_workflowController.State == CaptureProcessState.Error)
            {
                _gpioInterface.StartBlinkingSecondaryButton();
            }
            else
            {
                _gpioInterface.StopBlinkingSecondaryButton();
            }
        }
    }
}
