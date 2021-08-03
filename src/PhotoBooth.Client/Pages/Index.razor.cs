using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using PhotoBooth.Abstraction;

namespace PhotoBooth.Client.Pages
{
    public partial class Index : ComponentBase
    {
        [Inject]
        protected HttpClient HttpClient
        {
            get; set;
        }

        [Inject]
        protected ILogger<Index> Logger
        {
            get; set;
        }

        public string Cameras
        {
            get;
            set;
        }


        protected override async Task OnInitializedAsync()
        {
            try
            {
                List<CameraInfo> cameras = await HttpClient.GetFromJsonAsync<List<CameraInfo>>("api/Capture/State");
                Cameras = string.Join(Environment.NewLine, cameras);

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to update server state");
            }
        }
    }
}
