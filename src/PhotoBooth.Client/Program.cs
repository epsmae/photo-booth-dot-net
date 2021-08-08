using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Blazorise;
using Blazorise.Icons.Material;
using Blazorise.Material;
using Microsoft.JSInterop;

namespace PhotoBooth.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            builder.Services.AddBlazorise(options =>
            {
                options.ChangeTextOnKeyPress = true;
            })
            .AddMaterialProviders()
            .AddMaterialIcons();

            builder.Services.AddLocalization(opts => { opts.ResourcesPath = "Resources"; });
            //builder.Services.Configure<Request>(options =>
            //{
            //    // Define the list of cultures your app will support
            //    var supportedCultures = new List<CultureInfo>()
            //    {
            //        new CultureInfo("en-US"),
            //        new CultureInfo("de")
            //    };


            

            WebAssemblyHost host = builder.Build();

            ILogger<Program> logger = host.Services.GetService<ILogger<Program>>();

            logger.LogInformation("Starting....");
            IJSRuntime jsInterop = host.Services.GetRequiredService<IJSRuntime>();
            string language = await jsInterop.InvokeAsync<string>("getLanguage");

            logger.LogInformation($"Language: {language}");

            if (!string.IsNullOrEmpty(language))
            {
                CultureInfo culture = new CultureInfo(language);
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
            }

            await host.RunAsync();
        }
    }
}
