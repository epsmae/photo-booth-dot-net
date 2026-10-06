using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PhotoBooth.Abstraction;
using PhotoBooth.Camera;
using System.Linq;
using PhotoBooth.Abstraction.Configuration;
using PhotoBooth.Abstraction.LiveView;
using PhotoBooth.Camera.LiveView;
using PhotoBooth.Camera.LibGPhoto2;
using PhotoBooth.Abstraction.LibGPhoto2;
using PhotoBooth.Service.LiveView;
using PhotoBooth.Gpio;
using PhotoBooth.Printer;
using PhotoBooth.Service;

namespace PhotoBooth.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddSingleton<LiveViewHelper>();

#if DEBUG
            services.AddSingleton<IPrinterAdapter, PrinterAdapterSimulator>();
            services.AddSingleton<IUsbService, UsbServiceStub>();
            services.AddSingleton<IGpioInterface, GpioControllerStub>();
            services.AddSingleton<IHardwareController, HardwareController>();
#else
            services.AddSingleton<IPrinterAdapter, CupsPrinterAdapter>();
            
            services.AddSingleton<IUsbService, UsbService>();
            services.AddSingleton<IGpioInterface, GpioController>();
            services.AddSingleton<IHardwareController, HardwareController>();
#endif
            services.Configure<LiveViewOptions>(Configuration.GetSection(LiveViewOptions.SectionName));
            services.Configure<CameraDriverOptions>(Configuration.GetSection(CameraDriverOptions.SectionName));
            AddCamera(services, Configuration.GetSection(CameraDriverOptions.SectionName).Get<CameraDriverOptions>() ?? new CameraDriverOptions());
            services.AddSingleton<ILiveViewService, LiveViewService>();
            services.AddSingleton<IImageCombiner, ImageCombiner>();
            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<IImageResizer, ImageResizer>();
            services.AddSingleton<CaptureHub>();
            services.AddSingleton<NotificationService>();
            services.AddSingleton<IPrinterService, PrinterService>();
            services.AddSingleton<IFilePathProvider, FilePathProvider>();
            services.AddSingleton<Abstraction.Configuration.IConfigurationProvider, JsonConfigurationProvider>();
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddSingleton<IWorkflowController, WorkflowController>();
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddSignalR();
            // the Blazor framework files are served pre-compressed, binary payloads (images) do not compress well
            services.AddResponseCompression();
        }

        private static void AddCamera(IServiceCollection services, CameraDriverOptions options)
        {
#if DEBUG
            bool simulate = options.Simulate ?? true;
#else
            bool simulate = options.Simulate ?? false;
#endif
            if (options.Driver == CameraDriver.LibGPhoto2)
            {
                // one in process camera connection shared by live view and capture
                if (simulate)
                {
                    services.AddSingleton<IGPhoto2Api, SimulatedGPhoto2Api>();
                }
                else
                {
                    services.AddSingleton<IGPhoto2Api, GPhoto2Api>();
                }

                services.AddSingleton<CameraSession>();
                services.AddSingleton<ICameraService, LibGPhoto2CameraService>();
                services.AddSingleton<ILiveViewSource, LibGPhoto2LiveViewSource>();
                return;
            }

            // gphoto2 command line tool
            if (simulate)
            {
                services.AddSingleton<ICameraAdapter, CameraAdapterSimulator>();
            }
            else
            {
                services.AddSingleton<ICameraAdapter, GPhoto2CameraAdapter>();
            }

            services.AddSingleton<ICameraService, CameraService>();
            services.AddSingleton<ILiveViewSource>(CreateLiveViewSource);
        }

        private static ILiveViewSource CreateLiveViewSource(IServiceProvider serviceProvider)
        {
#if DEBUG
            string source = LiveViewOptions.SourceSimulator;
#else
            string source = LiveViewOptions.SourceGPhoto2;
#endif
            LiveViewOptions options = serviceProvider.GetRequiredService<IOptions<LiveViewOptions>>().Value;

            if (!string.IsNullOrEmpty(options.Source))
            {
                source = options.Source;
            }

            if (string.Equals(source, LiveViewOptions.SourceSimulator, StringComparison.OrdinalIgnoreCase))
            {
                return new SimulatedLiveViewSource();
            }

            return ActivatorUtilities.CreateInstance<GPhoto2LiveViewSource>(serviceProvider);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider, NotificationService notificationService, IHardwareController hardwareController)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseResponseCompression();
            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapHub<CaptureHub>("/capturehub");
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });

            notificationService.SendStateUpdate();

            hardwareController.Initialize();
        }
    }
}
