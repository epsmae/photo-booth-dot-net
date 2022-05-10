using System;
using System.Device.Gpio;
using System.Threading;

namespace PhotoBooth.Gpio
{
    internal class InputDebouncer
    {
        private readonly System.Device.Gpio.GpioController _controller;
        private readonly int _inputPinNumber;
        private readonly Action<bool> _action;

        public InputDebouncer(System.Device.Gpio.GpioController controller, int inputPinNumber, int debounceMs, Action<bool> action)
        {
            _controller = controller;
            _inputPinNumber = inputPinNumber;
            _action = action;
            Timer timer = new Timer(OnTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);

            _controller.RegisterCallbackForPinValueChangedEvent(inputPinNumber, PinEventTypes.Rising, (sender, eventArgs) =>
            {
                timer.Change(debounceMs, Timeout.Infinite);
            });

            _controller.RegisterCallbackForPinValueChangedEvent(inputPinNumber, PinEventTypes.Falling, (sender, eventArgs) =>
            {
                timer.Change(debounceMs, Timeout.Infinite);
            });
        }

        private void OnTimerElapsed(object state)
        {
            _action((bool)_controller.Read(_inputPinNumber));
        }
    }
}
