using System;
using Microsoft.Extensions.Logging;
using PhotoBooth.Abstraction;

namespace PhotoBooth.Gpio
{
    public class GpioControllerStub : IGpioInterface
    {
        private readonly ILogger<GpioControllerStub> _logger;
        public event EventHandler<bool> PrimaryButtonChanged;
        public event EventHandler<bool> SecondaryButtonChanged;


        public GpioControllerStub(ILogger<GpioControllerStub> logger)
        {
            _logger = logger;
        }
        
        public void StartBlinkingPrimaryButton()
        {
            _logger.LogInformation("Start blinking for primary button");
        }

        public void StopBlinkingPrimaryButton()
        {
            _logger.LogInformation("Stop blinking for primary button");
        }

        public void StartBlinkingSecondaryButton()
        {
            _logger.LogInformation("Start blinking for secondary button");
        }

        public void StopBlinkingSecondaryButton()
        {
            _logger.LogInformation("Stop blinking for secondary button");
        }
    }
}
