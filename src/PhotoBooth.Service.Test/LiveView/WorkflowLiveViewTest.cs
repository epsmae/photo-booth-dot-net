using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using PhotoBooth.Abstraction;
using PhotoBooth.Abstraction.Configuration;
using PhotoBooth.Abstraction.LiveView;
using PhotoBooth.Service.LiveView;

namespace PhotoBooth.Service.Test.LiveView
{
    public class WorkflowLiveViewTest : TestBase
    {
        [Test]
        public async Task TestLiveViewIsStoppedForCaptureAndRestartedWhenReady()
        {
            CameraServiceMock cameraServiceMock = new CameraServiceMock();
            FakeLiveViewSource source = new FakeLiveViewSource();
            using LiveViewService liveViewService = new LiveViewService(NullLogger<LiveViewService>.Instance, source, Options.Create(new LiveViewOptions()));

            bool? liveViewRunningDuringCapture = null;
            cameraServiceMock.OnCapture = () => liveViewRunningDuringCapture = liveViewService.IsRunning;

            IConfigurationService configService = new ConfigurationServiceMock().Object;
            FileService fileService = new FileService();
            ImageResizer imageResizer = new ImageResizer();

            WorkflowController controller = new WorkflowController(new ImageCombiner(fileService, imageResizer), loggerFactory.CreateLogger<WorkflowController>(),
                cameraServiceMock.Object, new PrinterServiceMock().Object, imageResizer, fileService, configService, liveViewService);

            await WaitFor(() => controller.State == CaptureProcessState.Ready, TimeSpan.FromSeconds(5));
            await WaitFor(() => liveViewService.IsRunning, TimeSpan.FromSeconds(5));

            await controller.Capture();
            await WaitFor(() => controller.State == CaptureProcessState.Review, TimeSpan.FromSeconds(10));

            Assert.AreEqual(false, liveViewRunningDuringCapture, "camera must be released before the capture");
            Assert.False(liveViewService.IsRunning, "no live view during review");

            await controller.Skip();
            await WaitFor(() => controller.State == CaptureProcessState.Ready, TimeSpan.FromSeconds(5));
            await WaitFor(() => liveViewService.IsRunning, TimeSpan.FromSeconds(5));
            await WaitFor(() => source.RunCount == 2, TimeSpan.FromSeconds(5));

            await liveViewService.StopAsync();
        }
    }
}
