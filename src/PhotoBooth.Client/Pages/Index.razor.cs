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

        public string PrinterQueue
        {
            get;
            set;
        }

        public string Printers
        {
            get;
            set;
        }


        protected override async Task OnInitializedAsync()
        {
            await FetchAvailableCameras();
            await FetchPrinters();
            await FetchPrinterQueue();
        }

        private async Task FetchAvailableCameras()
        {
            try
            {
                List<CameraInfo> cameras = await HttpClient.GetFromJsonAsync<List<CameraInfo>>("api/Camera/Cameras");
                Cameras = string.Join(Environment.NewLine, cameras);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to fetch cameras");
            }
        }

        private async Task FetchPrinters()
        {
            try
            {
                List<Printer> printers = await HttpClient.GetFromJsonAsync<List<Printer>>("api/Printer/Printers");
                Printers = string.Join(Environment.NewLine, printers);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed fetch printers");
            }
        }

        private async Task FetchPrinterQueue()
        {
            try
            {
                List<PrintQueueItem> queueItems = await HttpClient.GetFromJsonAsync<List<PrintQueueItem>>("api/Printer/PrinterQueue");
                PrinterQueue = string.Join(Environment.NewLine, queueItems);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed fetch printer queue items");
            }
        }

        private async Task ClearPrintQueue()
        {
            try
            {
                await HttpClient.PostAsJsonAsync("api/Printer/ClearPrintQueue", string.Empty);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to clear print queue");
            }

            await FetchPrinterQueue();
        }
    }
}
