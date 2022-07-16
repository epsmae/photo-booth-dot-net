using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PhotoBooth.Abstraction;
using PhotoBooth.Camera;
using System.Linq;
using PhotoBooth.Abstraction.Configuration;
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
            services.AddSingleton<ICameraAdapter, CameraAdapterSimulator>();
            services.AddSingleton<IPrinterAdapter, PrinterAdapterSimulator>();
            services.AddSingleton<IFileService, SampleFileService>();
            services.AddSingleton<IUsbService, UsbServiceStub>();
            services.AddSingleton<IGpioInterface, GpioControllerStub>();
            services.AddSingleton<IHardwareController, HardwareController>();
#else
            services.AddSingleton<ICameraAdapter, GPhoto2CameraAdapter>();
            services.AddSingleton<IPrinterAdapter, CupsPrinterAdapter>();
            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<IUsbService, UsbService>();
            services.AddSingleton<IGpioInterface, GpioController>();
            services.AddSingleton<IHardwareController, HardwareController>();
#endif

            services.AddSingleton<ICameraService, CameraService>();
            services.AddSingleton<IImageResizer, ImageResizer>();
            services.AddSingleton<CaptureHub>();
            services.AddSingleton<NotificationService>();
            services.AddSingleton<IPrinterService, PrinterService>();
            services.AddSingleton<IFilePathProvider, FilePathProvider>();
            services.AddSingleton<Abstraction.Configuration.IConfigurationProvider, JsonConfigurationProviderProvider>();
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddSingleton<IWorkflowController, WorkflowController>();
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddSignalR();
            services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });
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
