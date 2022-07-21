using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using PhotoBooth.Abstraction;
using PhotoBooth.Abstraction.Configuration;

namespace PhotoBooth.Service.Test
{
    public class WorkflowControllerTest : TestBase
    {
        private IWorkflowController _controller;
        private CameraServiceMock _cameraServiceMock;
        private PrinterServiceMock _printerServiceMock;

        private IList<CaptureProcessState> _traversedStates;
        private IList<int> _traversedCounts;
        private ILogger<ILogger<WorkflowControllerTest>> _logger;

        [SetUp]
        public async Task Setup()
        {
            _logger = loggerFactory.CreateLogger<ILogger<WorkflowControllerTest>>();
            _traversedStates = new List<CaptureProcessState>();
            _traversedCounts = new List<int>();

            _cameraServiceMock = new CameraServiceMock();
            _printerServiceMock = new PrinterServiceMock();


            IConfigurationService configService = new ConfigurationServiceMock().Object;
            _controller = new WorkflowController(loggerFactory.CreateLogger<WorkflowController>(), _cameraServiceMock.Object, _printerServiceMock.Object, new ImageResizer(), new FileServiceMock(), configService);
            _controller.CountDownChanged += OnCountDownChanged;
            _controller.StateChanged += OnStateChanged;

            await WaitForState(CaptureProcessState.Ready, 5);
            await WaitFor(()=> _traversedStates.Count == 1, TimeSpan.FromSeconds(5));

            _traversedStates.Clear();
        }
        
        [TearDown]
        public void TearDown()
        {
            _controller.CountDownChanged -= OnCountDownChanged;
            _controller.StateChanged -= OnStateChanged;
        }

        [Test]
        [Ignore("Only for doc generation")]
        public void TestCreateDiagram()
        {
            // can be visualized on https://sketchviz.com/new
            string diagram = _controller.GenerateUmlDiagram();
        }

        [Test]
        public async Task TestWorkflowSuccessfulWithoutPrint()
        {
            await _controller.Capture();

            await WaitForState(CaptureProcessState.CountDown, 5);
            
            await WaitForTraversedState(CaptureProcessState.Capture, 5);

            await WaitForTraversedState(CaptureProcessState.Review, 5);

            await WaitForTraversedState(CaptureProcessState.Ready, 5);
        }

        [Test]
        public async Task TestWorkflowSuccessfulWithPrint()
        {
            await _controller.Capture();

            await WaitForState(CaptureProcessState.CountDown, 5);

            await WaitForTraversedState(CaptureProcessState.Capture, 5);

            await WaitForState(CaptureProcessState.Review, 5);

            await _controller.Print();

            await WaitForTraversedState(CaptureProcessState.Print, 5);

            await WaitForState(CaptureProcessState.Ready, 5);
        }

        [Test]
        public async Task TestCameraError()
        {
            _cameraServiceMock.ThrowCaptureException();
            
            await _controller.Capture();

            await WaitForState(CaptureProcessState.CountDown, 5);

            await WaitForTraversedState(CaptureProcessState.Capture, 5);

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

            await WaitForTraversedState(CaptureProcessState.Capture, 5);

            await WaitForTraversedState(CaptureProcessState.Review, 5);

            await _controller.Print();

            await WaitForTraversedState(CaptureProcessState.Print, 5);

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
        public async Task TestWorkflowSuccessfulWithSkip()
        {
            await WaitForState(CaptureProcessState.Ready, 5);

            await _controller.Capture();

            await WaitForState(CaptureProcessState.CountDown, 5);

            await WaitForTraversedState(CaptureProcessState.Capture, 5);

            await WaitForState(CaptureProcessState.Review, 5);

            await _controller.Skip();

            await WaitForState(CaptureProcessState.Ready, 1);

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
            Assert.AreEqual(ConfigurationServiceMock.CountDownStepCount, _traversedCounts.Count - 1);

            for (int i = ConfigurationServiceMock.CountDownStepCount - 1; i >= 0; i--)
            {
                Assert.AreEqual(_traversedCounts[ConfigurationServiceMock.CountDownStepCount - (i+1)], i + 1);
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
            try
            {
                return WaitFor(() => _controller.State == state, TimeSpan.FromSeconds(timeoutInSeconds));
            }
            catch (Exception)
            {
                _logger.LogError($"Did not reach state={state} within {timeoutInSeconds}s");
                throw;
            }
            
        }

        private Task WaitForTraversedState(CaptureProcessState state, int timeoutInSeconds)
        {
            try
            {
                return WaitFor(() => _traversedStates.Contains(state), TimeSpan.FromSeconds(timeoutInSeconds));
            }
            catch (Exception)
            {
                _logger.LogError($"Did not reach state={state} within {timeoutInSeconds}s");
                throw;
            }
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