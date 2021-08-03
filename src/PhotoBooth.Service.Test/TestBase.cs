using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PhotoBooth.Service.Test
{
    public class TestBase
    {
        protected ILoggerFactory loggerFactory { get; set; }

        public TestBase()
        {
            ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder => builder.AddConsole());
            IServiceProvider provider = serviceCollection.BuildServiceProvider();
            loggerFactory = provider.GetService<ILoggerFactory>();
        }


        public static async Task WaitFor(Func<bool> waitFunc, TimeSpan timeout)
        {
            int msStep = 20;
            int timeoutMs = (int)timeout.TotalMilliseconds;

            int elapsedTime = 0;

            while (!waitFunc() && !(elapsedTime > timeoutMs))
            {
                await Task.Delay(msStep);
                elapsedTime += msStep;
            }

            if (!waitFunc())
            {
                throw new TimeoutException("Did not complete in time");
            }
        }
    }
}
