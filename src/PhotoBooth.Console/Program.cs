using System;
using System.CommandLine;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PhotoBooth.Abstraction;
using PhotoBooth.Camera;
using PhotoBooth.Printer;
using Serilog;

namespace PhotoBooth.Console
{
    public class Program
    {
        public const string DefaultOutputTemplate = "{Timestamp:dd.MM.yyyy HH:mm:ss.fff} {Level:u5}; {SourceContext} {Message:lj}{NewLine}{Exception}";

        public static async Task<int> Main(string[] args)
        {
            ServiceCollection services = new ServiceCollection();
            services.AddLogging(builder => builder.AddSerilog(SetupLogger().CreateLogger()));
            services.AddSingleton<ICameraService, CameraService>();
            services.AddSingleton<IPrinterService, PrinterService>();
#if DEBUG
            services.AddSingleton<IPrinterAdapter, PrinterAdapterSimulator>();
            services.AddSingleton<ICameraAdapter, CameraAdapterSimulator>();
#else
            services.AddSingleton<IPrinterAdapter, CupsPrinterAdapter>();
            services.AddSingleton<ICameraAdapter, GPhoto2CameraAdapter>();
#endif
            services.AddSingleton<PrintCommandHandler>();
            services.AddSingleton<CameraCommandHandler>();

            IServiceProvider serviceProvider = services.BuildServiceProvider();

            ILogger<Program> logger = serviceProvider.GetService<ILogger<Program>>();

            logger.LogInformation($"Photobooth console version={Assembly.GetExecutingAssembly().GetName().Version}");
            
            PrintCommandHandler printerCommandHandler = serviceProvider.GetService<PrintCommandHandler>();
            CameraCommandHandler cameraCommandHandler = serviceProvider.GetService<CameraCommandHandler>();
            
            RootCommand rootCommand = new RootCommand();

            foreach (Command command in printerCommandHandler.BuildPrintCommand())
            {
                rootCommand.AddCommand(command);
            }

            foreach (Command command in cameraCommandHandler.BuildPrintCommand())
            {
                rootCommand.AddCommand(command);
            }

            int resultCode = await rootCommand.InvokeAsync(args);
            
            logger.LogInformation($"Closing application with return code={resultCode}...");

            return resultCode;
        }

        private static LoggerConfiguration SetupLogger()
        {
            LoggerConfiguration config = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()

                .WriteTo.File("PhotoBooth.log",
                    retainedFileCountLimit: 5,
                    rollingInterval: RollingInterval.Day,
                    shared: false,
                    outputTemplate: DefaultOutputTemplate,
                    fileSizeLimitBytes: 5 * 1024 * 1024)

                .WriteTo.Console();

            return config;
        }
    }
}
