using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using PhotoBooth.Abstraction;
using PhotoBooth.Client.Models;

namespace PhotoBooth.Client.Pages
{
    public partial class Index : ComponentBase
    {
        private HubConnection _hubConnection;
        private string _capturedImageName;

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

        [Inject]
        protected NavigationManager Navigator
        {
            get; set;
        }

        public Printer SelectedPrinter
        {
            get;
            set;
        }

        public string SelectedStep
        {
            get;
            set;
        }

        public CameraInfo SelectedCamera
        {
            get;
            set;
        }

        public List<Printer> Printers
        {
            get;
            set;
        }

        public List<CameraInfo> Cameras
        {
            get;
            set;
        }

        protected override Task OnInitializedAsync()
        {
            SelectedStep = "step_camera";
            SelectedPrinter = null;
            SelectedCamera = null;
            Printers = new List<Printer>();
            Cameras = new List<CameraInfo>();

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(Navigator.ToAbsoluteUri("capturehub"))
                .WithAutomaticReconnect(new CustomRetryPolicy())
                .Build();
            return Task.CompletedTask;
        }
        private Task OnSelectedStepChanged(string name)
        {
            SelectedStep = name;

            return Task.CompletedTask;
        }



        private async Task ListCameras()
        {
            try
            {
                Cameras = await HttpClient.GetFromJsonAsync<List<CameraInfo>>("api/Camera/Cameras");
            }
            catch (Exception ex)
            {
                Cameras = new List<CameraInfo>();
                Logger.LogError(ex, "Failed to fetch cameras");
            }
        }

        private async Task CaptureImage()
        {
            try
            {
                await EnsureHubConnected();
                _capturedImageName = await _hubConnection.InvokeAsync<string>("CaptureImage", SelectedCamera.CameraModel);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed capture image");
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.DisposeAsync();
            }
        }


        private async Task EnsureHubConnected()
        {
            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                await _hubConnection.StartAsync();
            }
        }
        
        private async Task PrintImage()
        {
            try
            {
                await EnsureHubConnected();
                await _hubConnection.InvokeAsync<string>("PrintImage", SelectedPrinter.Name);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to print image");
            }
        }

        private async Task ListPrinters()
        {
            try
            {
                Printers = await HttpClient.GetFromJsonAsync<List<Printer>>("api/Printer/Printers");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed fetch printers");
                Printers = new List<Printer>();
            }
        }
    }
}
