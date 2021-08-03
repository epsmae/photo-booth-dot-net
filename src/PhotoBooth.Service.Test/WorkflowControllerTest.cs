using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using PhotoBooth.Abstraction;

namespace PhotoBooth.Service.Test
{
    public class WorkflowControllerTest : TestBase
    {
        private IWorkflowController _controller;
        private CameraServiceMock _cameraServiceMock;
        private PrinterServiceMock _printerServiceMock;

        private const int StepCountDownDurationMs = 200;
        private const int CountDownStepCount = 3;
        private const int ReviewStepCount = 10;

        private IList<CaptureProcessState> _traversedStates;
        private IList<int> _traversedCounts;

        [SetUp]
        public async Task Setup()
        {
            _traversedStates = new List<CaptureProcessState>();
            _traversedCounts = new List<int>();

            _cameraServiceMock = new CameraServiceMock();
            _printerServiceMock = new PrinterServiceMock();
            _controller = new WorkflowController(loggerFactory.CreateLogger<WorkflowController>(), _cameraServiceMock.Object, _printerServiceMock.Object, new ImageResizer(), new FileProviderMock());
            _controller.SetCountDown(CountDownStepCount);
            _controller.SetReviewDuration(ReviewStepCount);
            _controller.SetCountDownStepDuration(TimeSpan.FromMilliseconds(StepCountDownDurationMs));

            await WaitForState(CaptureProcessState.Ready, 5);

            _controller.CountDownChanged += OnCountDownChanged;
            _controller.StateChanged += OnStateChanged;
        }
        
        [TearDown]
        public void TearDown()
        {
            _controller.CountDownChanged -= OnCountDownChanged;
            _controller.StateChanged -= OnStateChanged;
        }

        [Test]
        public async Task TestWorkflowSuccessfulWithoutPrint()
        {
            await _controller.Capture();

            await WaitForState(CaptureProcessState.CountDown, 5);
            
            await WaitForState(CaptureProcessState.Capture, 5);

            await WaitForState(CaptureProcessState.Review, 5);

            await WaitForState(CaptureProcessState.Ready, 5);
        }

        [Test]
        public async Task TestWorkflowSuccessfulWithPrint()
        {
            await _controller.Capture();

            await WaitForState(CaptureProcessState.CountDown, 5);

            await WaitForState(CaptureProcessState.Capture, 5);

            await WaitForState(CaptureProcessState.Review, 5);

            await _controller.Print();

            await WaitForState(CaptureProcessState.Print, 5);

            await WaitForState(CaptureProcessState.Ready, 5);
        }

        [Test]
        public async Task TestCameraError()
        {
            _cameraServiceMock.ThrowCaptureException();
            
            await _controller.Capture();

            await WaitForState(CaptureProcessState.CountDown, 5);

            await WaitForState(CaptureProcessState.Capture, 5);
            
            await WaitForState(CaptureProcessState.Error, 5);

            await _controller.ConfirmError();

            await WaitForState(CaptureProcessState.Ready, 5);

            AssertTraversedStates(new List<CaptureProcessState>
            {
                CaptureProcessState.CountDown,
                CaptureProcessState.Capture,
                CaptureProcessState.Error,
                CaptureProcessState.Ready
            });
        }

        [Test]
        public async Task TestPrinterError()
        {
            _printerServiceMock.ThrowPrinterException();

            await _controller.Capture();

            await WaitForState(CaptureProcessState.CountDown, 5);

            await WaitForState(CaptureProcessState.Capture, 5);

            await WaitForState(CaptureProcessState.Review, 5);

            await _controller.Print();

            await WaitForState(CaptureProcessState.Print, 5);

            await WaitForState(CaptureProcessState.Error, 5);

            await _controller.ConfirmError();

            await WaitForState(CaptureProcessState.Ready, 5);

            AssertTraversedStates(new List<CaptureProcessState>
            {
                CaptureProcessState.CountDown,
                CaptureProcessState.Capture,
                CaptureProcessState.Review,
                CaptureProcessState.Print,
                CaptureProcessState.Error,
                CaptureProcessState.Ready
            });
        }



        [Test]
        public async Task TestWorkflowSuccessfulWithProceed()
        {
            await WaitForState(CaptureProcessState.Ready, 5);

            await _controller.Capture();

            await WaitForState(CaptureProcessState.CountDown, 5);

            await WaitForState(CaptureProcessState.Capture, 5);

            await WaitForState(CaptureProcessState.Review, 5);

            await _controller.Capture();

            await WaitForState(CaptureProcessState.Ready, 5);

            AssertTraversedStates(new List<CaptureProcessState>
            {
                CaptureProcessState.CountDown,
                CaptureProcessState.Capture,
                CaptureProcessState.Review,
                CaptureProcessState.Ready
            });
        }

        [Test]
        public async Task TestCountDownEvent()
        {
            await _controller.Capture();

            await WaitForState(CaptureProcessState.CountDown, 5);

            await WaitForState(CaptureProcessState.Capture, 5);

            AssertCountDownEvents();
        }

        private void AssertCountDownEvents()
        {
            Assert.AreEqual(CountDownStepCount, _traversedCounts.Count - 1);

            for (int i = CountDownStepCount -1; i >= 0; i--)
            {
                Assert.AreEqual(_traversedCounts[CountDownStepCount -(i+1)], i + 1);
            }
        }

        private void AssertTraversedStates(List<CaptureProcessState> expectedTraversedStates)
        {
            Assert.AreEqual(expectedTraversedStates.Count, _traversedStates.Count);

            for (int i = 0; i < expectedTraversedStates.Count; i++)
            {
                Assert.AreEqual(expectedTraversedStates[i], _traversedStates[i]);
            }
        }

        private Task WaitForState(CaptureProcessState state, int timeoutInSeconds)
        {
            return WaitFor(() => _controller.State == state, TimeSpan.FromSeconds(timeoutInSeconds));
        }

        private void OnStateChanged(object? sender, EventArgs e)
        {
            _traversedStates.Add(_controller.State);
        }

        private void OnCountDownChanged(object? sender, EventArgs e)
        {
            _traversedCounts.Add(_controller.CurrentCountDownStep);
        }
    }
}