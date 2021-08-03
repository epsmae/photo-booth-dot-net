using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using PhotoBooth.Abstraction;

namespace PhotoBooth.Client.Pages
{
    public partial class About : ComponentBase
    {

        [Inject]
        protected HttpClient HttpClient
        {
            get; set;
        }

        [Inject]
        protected ILogger<About> Logger
        {
            get; set;
        }


        protected string ServerVersion
        {
            get;
            set;
        }

        protected string ClientVersion
        {
            get;
            set;
        }

        protected override async Task OnInitializedAsync()
        {


            try
            {
                ClientVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                ServerVersion = await HttpClient.GetStringAsync("api/About/version");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to get version");
            }
        }
    }
}
