using System.Device.Gpio;
using System.Threading;

namespace PhotoBooth.Gpio
{
    internal class BlinkController
    {
        private readonly System.Device.Gpio.GpioController _controller;
        private readonly int _outputPinNumber;
        private readonly bool _initialState;
        private readonly Timer _timer;
        private bool _state;
        private readonly int _sequenceDuration;

        public BlinkController(System.Device.Gpio.GpioController controller, int outputPinNumber, int blinkPeriod, bool initialState)
        {
            _controller = controller;
            _outputPinNumber = outputPinNumber;
            _initialState = initialState;
            _state = initialState;
            _sequenceDuration = blinkPeriod / 2;
            _timer = new Timer(OnTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);
            _controller.Write(_outputPinNumber, _initialState);
        }

        private void OnTimerElapsed(object state)
        {
            _state = !_state;
            _controller.Write(_outputPinNumber, _state);
        }
        
        public void Start()
        {
            _state = _initialState;
            _controller.Write(_outputPinNumber, _initialState);
            _timer.Change(_sequenceDuration, _sequenceDuration);
        }

        public void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            _controller.Write(_outputPinNumber, _initialState);
        }
    }
}
