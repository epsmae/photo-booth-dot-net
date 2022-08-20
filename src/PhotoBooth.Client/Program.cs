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
using MudBlazor.Services;
using PhotoBooth.Client.Extensions;

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
                //options.ChangeTextOnKeyPress = true;
            })
            .AddMaterialProviders()
            .AddMaterialIcons();

            builder.Services.AddMudServices();

            builder.Services.AddLocalization(opts => { opts.ResourcesPath = "Resources"; });
            
            WebAssemblyHost host = builder.Build();
            
            ILogger<Program> logger = host.Services.GetService<ILogger<Program>>();
            logger.LogInformation("Starting....");
            await host.SetDefaultCulture(new List<string> {"de", "en"}, "en");

            await host.RunAsync();
        }
    }
}
