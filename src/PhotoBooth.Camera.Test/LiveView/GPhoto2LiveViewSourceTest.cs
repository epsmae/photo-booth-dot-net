using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using PhotoBooth.Abstraction.LiveView;
using PhotoBooth.Camera.LiveView;

namespace PhotoBooth.Camera.Test.LiveView
{
    /// <summary>
    /// Runs the real gphoto2 live view code against tools/fake-gphoto2 (python script).
    /// </summary>
    [Platform(Exclude = "Win")]
    [NonParallelizable]
    public class GPhoto2LiveViewSourceTest
    {
        private string _fakeGPhoto2;

        [SetUp]
        public void Setup()
        {
            _fakeGPhoto2 = Path.Combine(TestContext.CurrentContext.TestDirectory, "FakeGPhoto2", "gphoto2");
            if (!OperatingSystem.IsWindows())
            {
                // the copy to the output directory does not preserve the executable flag
                File.SetUnixFileMode(_fakeGPhoto2, File.GetUnixFileMode(_fakeGPhoto2) | UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute);
            }
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("FAKE_GPHOTO2_ERROR", null);
            Environment.SetEnvironmentVariable("FAKE_GPHOTO2_IGNORE_SIGINT", null);
        }

        [Test]
        public async Task TestFramesAndGracefulStop()
        {
            int frames = 0;
            using CancellationTokenSource stop = new CancellationTokenSource();
            using CancellationTokenSource kill = new CancellationTokenSource();

            Task run = CreateSource().RunAsync(_ => Interlocked.Increment(ref frames), stop.Token, kill.Token);

            await WaitFor(() => Volatile.Read(ref frames) >= 5, TimeSpan.FromSeconds(10));

            Stopwatch stopwatch = Stopwatch.StartNew();
            stop.Cancel();

            await Assert.ThrowsAsync(Is.InstanceOf<OperationCanceledException>(), async () => await run);
            Assert.Less(stopwatch.ElapsedMilliseconds, 3000, "gphoto2 should exit quickly after SIGINT");
            Assert.False(kill.IsCancellationRequested);
        }

        [Test]
        public async Task TestLiveViewErrorIsReported()
        {
            Environment.SetEnvironmentVariable("FAKE_GPHOTO2_ERROR", "Liveview cannot start: Exposure Program Mode is not P/A/S/M");

            LiveViewException exception = await Assert.ThrowsAsync<LiveViewException>(async () =>
                await CreateSource().RunAsync(_ => { }, CancellationToken.None, CancellationToken.None));

            StringAssert.Contains("Exposure Program Mode is not P/A/S/M", exception.Message);
        }

        [Test]
        public async Task TestKillWhenSigIntIsIgnored()
        {
            Environment.SetEnvironmentVariable("FAKE_GPHOTO2_IGNORE_SIGINT", "1");

            int frames = 0;
            using CancellationTokenSource stop = new CancellationTokenSource();
            using CancellationTokenSource kill = new CancellationTokenSource();

            Task run = CreateSource().RunAsync(_ => Interlocked.Increment(ref frames), stop.Token, kill.Token);
            await WaitFor(() => Volatile.Read(ref frames) >= 2, TimeSpan.FromSeconds(10));

            stop.Cancel();
            await Task.Delay(500);
            Assert.False(run.IsCompleted, "process ignores SIGINT and keeps running");

            kill.Cancel();
            await Assert.ThrowsAsync(Is.InstanceOf<OperationCanceledException>(), async () => await run.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private GPhoto2LiveViewSource CreateSource()
        {
            return new GPhoto2LiveViewSource(NullLogger<GPhoto2LiveViewSource>.Instance, Options.Create(new LiveViewOptions {GPhoto2Path = _fakeGPhoto2}));
        }

        private static async Task WaitFor(Func<bool> condition, TimeSpan timeout)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            while (!condition())
            {
                if (stopwatch.Elapsed > timeout)
                {
                    Assert.Fail("Timeout");
                }

                await Task.Delay(20);
            }
        }
    }
}
