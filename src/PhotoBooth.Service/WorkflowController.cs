using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PhotoBooth.Abstraction;
using PhotoBooth.Abstraction.Configuration;
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

        private const double DefaultStepDownDurationInSeconds = 1;
        private const double DefaultReviewCountDownStepCount = 10;
        private const double DefaultCaptureCountDownStepCount = 3;
        // Raspberry pi touch 7" has width of 800px and side bar 50px
        private const int DefaultPreviewImageWidth = 750;


        private readonly ILogger<WorkflowController> _logger;
        private readonly ICameraService _cameraService;
        private readonly IPrinterService _printerService;
        private readonly IImageResizer _imageResizer;
        private readonly IFileProvider _fileProvider;
        private readonly IConfigurationService _configurationService;
        private CaptureStates _state = CaptureStates.Initializing;



        private readonly StateMachine<CaptureStates, CaptureTriggers> _machine;

        private TimeSpan _countDownStepDuration;
        private int _currentCaptureCountDownStep;
        private int _currentReviewCountDownStep;

        private readonly Timer _countDownTimer;
        private readonly Timer _reviewTimer;
        private Exception _lastException;
        private CaptureResult _captureResult;
        private byte[] _currentImageData;
        
        public WorkflowController(ILogger<WorkflowController> logger, ICameraService cameraService, IPrinterService printerService, IImageResizer imageResizer, IFileProvider fileProvider, IConfigurationService configurationService)
        {
            _logger = logger;
            _cameraService = cameraService;
            _printerService = printerService;
            _imageResizer = imageResizer;
            _fileProvider = fileProvider;
            _configurationService = configurationService;

            _countDownTimer = new Timer(OnCountDownTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);
            _reviewTimer = new Timer(OnReviewCountDownTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);
            
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
                await RegisterSettings();
                await InitializeCamera();

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
                    string selectedPrinter = _configurationService.SelectedPrinter;

                    if (string.IsNullOrEmpty(selectedPrinter) && !await TrySetDefaultPrinter())
                    {
                        throw new NoPrinterAvailableException("No printer found");
                    }
                    
                    await _printerService.Print(_configurationService.SelectedPrinter, _captureResult.FileName);

                    await _machine.FireAsync(CaptureTriggers.PrintCompleted);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to print image");
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
                    string selectedCamera = _configurationService.SelectedCamera;

                    if (string.IsNullOrEmpty(selectedCamera) && !await TrySetDefaultCamera())
                    {
                        throw new NoCameraAvailableException("No camera found");
                    }



                    _captureResult = await _cameraService.CaptureImage(_fileProvider.PhotoDirectory, _configurationService.SelectedCamera);

                   using (Stream stream = _fileProvider.OpenFile(_captureResult.FileName))
                   {
                       _currentImageData = _imageResizer.ResizeImage(stream, _configurationService.ReviewImageWidth);
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
                return _currentCaptureCountDownStep;
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
            _countDownStepDuration = TimeSpan.FromSeconds(_configurationService.StepDownDurationInSeconds);
            _currentCaptureCountDownStep = _configurationService.CaptureCountDownStepCount;
            _currentReviewCountDownStep = _configurationService.ReviewCountDownStepCount;
            return _machine.FireAsync(CaptureTriggers.Capture);
        }

        public Task Print()
        {
            return _machine.FireAsync(CaptureTriggers.Print);
        }
        
        private void OnCountDownTimerElapsed(object state)
        {
            try
            {
                _currentCaptureCountDownStep--;

                NotifyCountDownChanged();

                if (_currentCaptureCountDownStep == 0)
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
            NotifyCountDownChanged();
            _countDownTimer.Change((int)_countDownStepDuration.TotalMilliseconds, Timeout.Infinite);
        }

        private void StartReviewTimer()
        {
            _reviewTimer.Change((int) _countDownStepDuration.TotalMilliseconds, Timeout.Infinite);
        }

        private async Task InitializeCamera()
        {
            try
            {
                await _cameraService.Initialize();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize");
            }
        }

        private async Task RegisterSettings()
        {
            try
            {
                _configurationService.Register(ConfigurationKeys.CaptureCountDownStepCount, DefaultCaptureCountDownStepCount);
                _configurationService.Register(ConfigurationKeys.ReviewCountDownStepCount, DefaultReviewCountDownStepCount);
                _configurationService.Register(ConfigurationKeys.StepDownDurationInSeconds, DefaultStepDownDurationInSeconds);
                _configurationService.Register(ConfigurationKeys.ReviewImageWidth, DefaultPreviewImageWidth);

                await TrySetInitialDefaultCamera();
                await TrySetInitialDefaultPrinter();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set default settings values");
            }
        }

        private async Task TrySetInitialDefaultPrinter()
        {
            List<Printer> printers = await _printerService.ListPrinters();

            if (printers.Any())
            {
                _configurationService.Register(ConfigurationKeys.SelectedPrinter, printers.First().Name);
            }
            else
            {
                _configurationService.Register(ConfigurationKeys.SelectedPrinter, string.Empty);
            }
        }

        private async Task TrySetInitialDefaultCamera()
        {
            List<CameraInfo> cameras = await _cameraService.ListCameras();

            if (cameras.Any())
            {
                _configurationService.Register(ConfigurationKeys.SelectedCamera, cameras.First().CameraModel);
            }
            else
            {
                _configurationService.Register(ConfigurationKeys.SelectedCamera, string.Empty);
            }
        }


        private async Task<bool> TrySetDefaultPrinter()
        {
            try
            {
                List<PrintQueueItem> printers = await _printerService.ListPrintQueue();
                if (printers.Any())
                {
                    _configurationService.Register(ConfigurationKeys.SelectedPrinter, printers.First().Name);
                }

                return true;
            }
            catch (Exception)
            {
                // ignore
            }

            return false;
        }

        private async Task<bool> TrySetDefaultCamera()
        {
            try
            {
                List<CameraInfo> cameras = await _cameraService.ListCameras();
                if (cameras.Any())
                {
                    _configurationService.Register(ConfigurationKeys.SelectedCamera, cameras.First().CameraModel);
                }

                return true;
            }
            catch (Exception)
            {
                // ignore
            }

            return false;
        }
    }
}
