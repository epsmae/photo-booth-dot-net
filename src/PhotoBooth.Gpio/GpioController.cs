using System;
using System.Device.Gpio;
using Microsoft.Extensions.Logging;
using PhotoBooth.Abstraction;

namespace PhotoBooth.Gpio
{
    public class GpioController : IGpioInterface
    {
        private readonly ILogger<GpioController> _logger;
        private readonly BlinkController _blinkControllerPrimary;
        private readonly BlinkController _blinkControllerSecondary;
        private readonly InputDebouncer _primaryDebouncer;
        private readonly InputDebouncer _secondaryDebouner;
        public event EventHandler<bool> PrimaryButtonChanged;
        public event EventHandler<bool> SecondaryButtonChanged;

        private const int DebounceTime = 200;

        private const int PinRelay1 = 37;
        private const int PinRelay2 = 38;
        private const int PinRelay3 = 40;

        private const int PinButton1 = 24;
        private const int PinButton2 = 26;

        public GpioController(ILogger<GpioController> logger)
        {
            _logger = logger;
            System.Device.Gpio.GpioController gpioController = new System.Device.Gpio.GpioController(PinNumberingScheme.Board);

            gpioController.OpenPin(PinRelay1, PinMode.Output);
            gpioController.OpenPin(PinRelay2, PinMode.Output);
            gpioController.OpenPin(PinRelay3, PinMode.Output);

            gpioController.OpenPin(PinButton1, PinMode.InputPullUp);
            gpioController.OpenPin(PinButton2, PinMode.InputPullUp);


            _blinkControllerPrimary = new BlinkController(gpioController, PinRelay1, 1000, true);
            _blinkControllerSecondary = new BlinkController(gpioController, PinRelay2, 1000, true);

            _primaryDebouncer = new InputDebouncer(gpioController, PinButton1, DebounceTime, value =>
            {
                _logger.LogInformation($"Primary button changed value={value}");

                PrimaryButtonChanged?.Invoke(this, value);
            });

            _secondaryDebouner = new InputDebouncer(gpioController, PinButton2, DebounceTime, value =>
            {
                _logger.LogInformation($"Secondary button changed value={value}");

                SecondaryButtonChanged?.Invoke(this, value);
            });
        }
        
        public void StartBlinkingPrimaryButton()
        {
            _logger.LogInformation($"Start blinking for primary button");
            _blinkControllerPrimary.Start();
        }

        public void StopBlinkingPrimaryButton()
        {
            _logger.LogInformation($"Stop blinking for primary button");
            _blinkControllerPrimary.Stop();
        }

        public void StartBlinkingSecondaryButton()
        {
            _logger.LogInformation($"Start blinking for secondary button");
            _blinkControllerSecondary.Start();
        }

        public void StopBlinkingSecondaryButton()
        {
            _logger.LogInformation($"Stop blinking for secondary button");
            _blinkControllerSecondary.Stop();
        }
    }
}
