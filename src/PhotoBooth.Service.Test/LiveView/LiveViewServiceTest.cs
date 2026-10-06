using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using PhotoBooth.Abstraction.LiveView;
using PhotoBooth.Service.LiveView;

namespace PhotoBooth.Service.Test.LiveView
{
    public class LiveViewServiceTest
    {
        private FakeLiveViewSource _source;

        [SetUp]
        public void Setup()
        {
            _source = new FakeLiveViewSource();
        }

        [Test]
        public async Task TestFramesAndStop()
        {
            using LiveViewService service = CreateService();
            await service.StartAsync();

            Assert.True(service.IsRunning);

            LiveViewFrame first = await service.WaitForFrameAsync(0, Timeout());
            LiveViewFrame second = await service.WaitForFrameAsync(first.Id, Timeout());
            Assert.Greater(second.Id, first.Id);

            // a waiting stream client is released with null when the live view stops
            Task<LiveViewFrame> waiting = service.WaitForFrameAsync(long.MaxValue, Timeout());
            await service.StopAsync();

            Assert.False(service.IsRunning);
            Assert.IsNull(await waiting);
            Assert.IsNull(await service.WaitForFrameAsync(0, Timeout()));
        }

        [Test]
        public async Task TestStartIsIdempotent()
        {
            using LiveViewService service = CreateService();
            await service.StartAsync();
            await service.WaitForFrameAsync(0, Timeout());
            await service.StartAsync();
            await service.WaitForFrameAsync(1, Timeout());

            Assert.AreEqual(1, _source.RunCount);
            await service.StopAsync();
        }

        [Test]
        public async Task TestStartCondition()
        {
            using LiveViewService service = CreateService();
            await service.StartAsync(() => false);

            Assert.False(service.IsRunning);
            Assert.AreEqual(0, _source.RunCount);
        }

        [Test]
        public async Task TestDisabled()
        {
            using LiveViewService service = CreateService(new LiveViewOptions {Enabled = false});
            await service.StartAsync();

            Assert.False(service.IsRunning);
            Assert.False(service.GetStatus().Enabled);
        }

        [Test]
        public async Task TestKillWhenStopIsIgnored()
        {
            _source.IgnoreStop = true;
            using LiveViewService service = CreateService(new LiveViewOptions {StopTimeoutMilliseconds = 200});
            await service.StartAsync();
            await service.WaitForFrameAsync(0, Timeout());

            Stopwatch stopwatch = Stopwatch.StartNew();
            await service.StopAsync();

            Assert.False(service.IsRunning);
            Assert.GreaterOrEqual(stopwatch.ElapsedMilliseconds, 190);
            Assert.Less(stopwatch.ElapsedMilliseconds, 2000);
        }

        [Test]
        public async Task TestSourceErrorAndRestart()
        {
            _source.FailWith = new LiveViewException("Liveview cannot start: Battery exhausted");
            using LiveViewService service = CreateService();

            await service.StartAsync();
            await TestBase.WaitFor(() => !service.IsRunning, TimeSpan.FromSeconds(5));
            Assert.AreEqual("Liveview cannot start: Battery exhausted", service.GetStatus().LastError);

            _source.FailWith = null;
            await service.StartAsync();

            Assert.True(service.IsRunning);
            Assert.IsNull(service.GetStatus().LastError);
            await TestBase.WaitFor(() => _source.RunCount == 2, TimeSpan.FromSeconds(5));
            await service.StopAsync();
        }

        [Test]
        public async Task TestIdleTimeout()
        {
            using LiveViewService service = CreateService(new LiveViewOptions {IdleTimeoutSeconds = 1});
            await service.StartAsync();

            await TestBase.WaitFor(() => !service.IsRunning, TimeSpan.FromSeconds(5));
        }

        [Test]
        public void TestViewers()
        {
            using LiveViewService service = CreateService();

            IDisposable viewer = service.RegisterViewer();
            Assert.AreEqual(1, service.GetStatus().Viewers);

            viewer.Dispose();
            viewer.Dispose();
            Assert.AreEqual(0, service.GetStatus().Viewers);
        }

        private LiveViewService CreateService(LiveViewOptions options = null)
        {
            return new LiveViewService(NullLogger<LiveViewService>.Instance, _source, Options.Create(options ?? new LiveViewOptions()));
        }

        private static CancellationToken Timeout()
        {
            return new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;
        }
    }
}
