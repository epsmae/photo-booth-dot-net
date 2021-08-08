using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PhotoBooth.Abstraction;
using PhotoBooth.Abstraction.Exceptions;
using Stateless;

namespace PhotoBooth.Service
{
    public class WorkflowController : IWorkflowController
    {
        public string CurrentImageFileName
        {
            get
            {
                if (!string.IsNullOrEmpty(_captureResult?.FileName))
                {
                    return _captureResult.FileName;
                }

                return string.Empty;
            }
        }

        public byte[] ImageData
        {
            get
            {
                return _currentImageData;
            }
        }

        public event EventHandler StateChanged;
        public event EventHandler CountDownChanged;
        public event EventHandler ReviewCountDownChanged;

        private readonly ILogger<WorkflowController> _logger;
        private readonly ICameraService _cameraService;
        private readonly IPrinterService _printerService;
        private readonly IImageResizer _imageResizer;
        private readonly IFileProvider _fileProvider;
        private CaptureStates _state = CaptureStates.Initializing;

        //private const int MinimalReviewDurationMs = 200;
        private const int MinimalCountDownStepDurationMs = 200;
        private const int MinimalCountDownSteps = 3;
        private const int MinimalReviewCountDownSteps = 5;

        // Raspberry pi touch 7" has width of 800px and side bar 50px
        private const int PreviewImageWidth = 750;

        private readonly StateMachine<CaptureStates, CaptureTriggers> _machine;

        //private TimeSpan _reviewDuration;
        private TimeSpan _countDownStepDuration;
        private int _countDownStepCount;
        private int _currentCountDownStep;
        private int _reviewCountDown;
        private int _currentReviewCountDownStep;

        private readonly Timer _countDownTimer;
        private readonly Timer _reviewTimer;
        private Exception _lastException;
        private CaptureResult _captureResult;
        private byte[] _currentImageData;

        public WorkflowController(ILogger<WorkflowController> logger, ICameraService cameraService, IPrinterService printerService, IImageResizer imageResizer, IFileProvider fileProvider)
        {
            _logger = logger;
            _cameraService = cameraService;
            _printerService = printerService;
            _imageResizer = imageResizer;
            _fileProvider = fileProvider;

            _countDownTimer = new Timer(OnCountDownTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);
            _reviewTimer = new Timer(OnReviewCountDownTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);
            //_reviewDuration = TimeSpan.FromSeconds(15);
            _countDownStepDuration = TimeSpan.FromSeconds(1);
            _countDownStepCount = 3;
            _reviewCountDown = 15;

            _machine = new StateMachine<CaptureStates, CaptureTriggers>(() => _state, s => _state = s);

            _machine.Configure(CaptureStates.Processing)
                .Permit(CaptureTriggers.Error, CaptureStates.Error);

            _machine.Configure(CaptureStates.Error)
                .Permit(CaptureTriggers.ConfirmError, CaptureStates.Ready);

            _machine.Configure(CaptureStates.Initializing)
                .OnEntry(() => StartInitialization())
                .OnActivate(() => StartInitialization())
                .Permit(CaptureTriggers.InitializationDone, CaptureStates.Ready);


            _machine.Configure(CaptureStates.Ready)
                .SubstateOf(CaptureStates.Processing)
                .Permit(CaptureTriggers.Capture, CaptureStates.CountDown);

            _machine.Configure(CaptureStates.CountDown)
                .SubstateOf(CaptureStates.Processing)
                .OnEntry(() => StartCountDownTimer())
                .Permit(CaptureTriggers.CountdownElapsed, CaptureStates.Capture);

            _machine.Configure(CaptureStates.Capture)
                .SubstateOf(CaptureStates.Processing)
                .OnEntry(()=> StartCaptureImage())
                .Permit(CaptureTriggers.CaptureCompleted, CaptureStates.Review);

            _machine.Configure(CaptureStates.Review)
                .SubstateOf(CaptureStates.Processing)
                .OnEntry(() => StartReviewTimer())
                .OnExit(()=> StopReviewTimer())
                .Permit(CaptureTriggers.ReviewCountDownElapsed, CaptureStates.Ready)
                .Permit(CaptureTriggers.Print, CaptureStates.Print)
                .Permit(CaptureTriggers.Capture, CaptureStates.Ready);

            _machine.Configure(CaptureStates.Print)
                .SubstateOf(CaptureStates.Processing)
                .OnEntry(() => StartPrint())
                .Permit(CaptureTriggers.PrintCompleted, CaptureStates.Ready);

            _machine.OnTransitioned(t =>
            {
                _logger.LogInformation($"OnTransitioned: {t.Source} -> {t.Destination} via {t.Trigger}({string.Join(", ", t.Parameters)})");

                try
                {
                    if (StateChanged != null)
                    {
                        StateChanged(this, EventArgs.Empty);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to notify state changed");
                }
            });

            _machine.Activate();
        }

        private void StartInitialization()
        {
            Task.Run(async () =>
            {
                try
                {
                    await _cameraService.Initialize();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to initialize");
                }

                await _machine.FireAsync(CaptureTriggers.InitializationDone);
            });
        }

        private void StopReviewTimer()
        {
            _reviewTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public Task ConfirmError()
        {
            return _machine.FireAsync(CaptureTriggers.ConfirmError);
        }

        private void StartPrint()
        {
            Task.Run(async () =>
            {
                try
                {
                    await _printerService.Print("Canon_SELPHY_CP1300", _captureResult.FileName);

                    await _machine.FireAsync(CaptureTriggers.PrintCompleted);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to capture image");
                    _lastException = ex;
                    await _machine.FireAsync(CaptureTriggers.Error);
                }
            });
        }

        private void StartCaptureImage()
        {
            Task.Run(async() =>
            {
                try
                {
                    _captureResult = await _cameraService.CaptureImage();


                    
                   using (Stream stream = _fileProvider.OpenFile(_captureResult.FileName))
                   {
                       _currentImageData = _imageResizer.ResizeImage(stream, PreviewImageWidth);
                   }

                   await _machine.FireAsync(CaptureTriggers.CaptureCompleted);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to capture image");
                    _lastException = ex;
                    _captureResult = null;
                    await _machine.FireAsync(CaptureTriggers.Error);
                }
            });
        }

        public CaptureProcessState State
        {
            get
            {
                switch (_state)
                {
                    case CaptureStates.Initializing:
                        return CaptureProcessState.Initializing;
                    case CaptureStates.CountDown:
                        return CaptureProcessState.CountDown;
                    case CaptureStates.Error:
                        return CaptureProcessState.Error;
                    case CaptureStates.Capture:
                        return CaptureProcessState.Capture;
                    case CaptureStates.Review:
                        return CaptureProcessState.Review;
                    case CaptureStates.Print:
                        return CaptureProcessState.Print;
                    default:
                        return CaptureProcessState.Ready;
                }
            }
        }

        public int CurrentCountDownStep
        {
            get
            {
                return _currentCountDownStep;
            }
        }

        public Exception LastException
        {
            get
            {
                return _lastException;
            }
        }

        public int CurrentReviewCountDown
        {
            get
            {
                return _currentReviewCountDownStep;
            }
        }

        public Task Capture()
        {
            _currentCountDownStep = _countDownStepCount;
            _currentReviewCountDownStep = _reviewCountDown;
            return _machine.FireAsync(CaptureTriggers.Capture);
        }

        public Task Print()
        {
            return _machine.FireAsync(CaptureTriggers.Print);
        }


        public void SetReviewDuration(int stepCount)
        {
            if (stepCount < MinimalReviewCountDownSteps)
            {
                throw new ArgumentException($"Review duration has to bo larger or equal as {MinimalReviewCountDownSteps}");
            }

            if (!(_machine.State == CaptureStates.Initializing || _machine.State == CaptureStates.Initializing))
            {
                throw new InvalidStateException($"Review duration can only be set in state={CaptureStates.Ready}");
            }

            _reviewCountDown = stepCount;
        }



        public void SetCountDown(int stepCount)
        {
            if (stepCount < MinimalCountDownSteps)
            {
                throw new ArgumentException($"Countdown steps has to bo larger or equal as {MinimalCountDownSteps}");
            }
            
            if (! (_machine.State == CaptureStates.Initializing || _machine.State == CaptureStates.Initializing))
            {
                throw new InvalidStateException($"Countdown duration can only be set in state={CaptureStates.Ready}");
            }

            _countDownStepCount = stepCount;
        }

        public void SetCountDownStepDuration(TimeSpan stepDuration)
        {
            if (stepDuration.TotalMilliseconds < MinimalCountDownStepDurationMs)
            {
                throw new ArgumentException($"Countdown steps duration has to be larger or equal as {MinimalCountDownStepDurationMs}");
            }

            if (!(_machine.State == CaptureStates.Initializing || _machine.State == CaptureStates.Initializing))
            {
                throw new InvalidStateException($"Countdown duration can only be set in state={CaptureStates.Ready}");
            }

            _countDownStepDuration = stepDuration;
        }

        //private void OnReviewTimerElapsed(object state)
        //{
        //    try
        //    {
        //        _machine.Fire(CaptureTriggers.ReviewCountDownElapsed);
        //    }
        //    catch
        //    {
        //        // ignore invalid state exception
        //    }
        //}

        private void OnCountDownTimerElapsed(object state)
        {
            try
            {
                _currentCountDownStep--;

                NotifyCountDownChanged();

                if (_currentCountDownStep == 0)
                {
                    _machine.Fire(CaptureTriggers.CountdownElapsed);
                }
                else
                {
                    _countDownTimer.Change((int)_countDownStepDuration.TotalMilliseconds, Timeout.Infinite);
                }
            }
            catch
            {
                // ignore invalid state exception
            }
        }

        private void OnReviewCountDownTimerElapsed(object state)
        {
            try
            {
                _currentReviewCountDownStep--;

                NotifyReviewCountDownChanged();

                if (_currentReviewCountDownStep == 0)
                {
                    _machine.Fire(CaptureTriggers.ReviewCountDownElapsed);
                }
                else
                {
                    _reviewTimer.Change((int) _countDownStepDuration.TotalMilliseconds, Timeout.Infinite);
                }
            }
            catch
            {
                // ignore invalid state exception
            }
        }

        private void NotifyCountDownChanged()
        {
            try
            {
                if (CountDownChanged != null)
                {
                    CountDownChanged(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to notify countdown changed");
            }
        }

        private void NotifyReviewCountDownChanged()
        {
            try
            {
                if (ReviewCountDownChanged != null)
                {
                    ReviewCountDownChanged(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to notify review countdown changed");
            }
        }

        private void StartCountDownTimer()
        {
            _currentCountDownStep = _countDownStepCount;
            NotifyCountDownChanged();
            _countDownTimer.Change((int)_countDownStepDuration.TotalMilliseconds, Timeout.Infinite);
        }

        private void StartReviewTimer()
        {
            _reviewTimer.Change((int) _countDownStepDuration.TotalMilliseconds, Timeout.Infinite);
        }
    }
}
