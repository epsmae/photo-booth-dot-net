using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PhotoBooth.Abstraction;

namespace PhotoBooth.Client.Shared
{
    public partial class PrinterQueueDialog : ComponentBase
    {

        public PrinterQueueDialog()
        {
            PrinterQueue = new List<PrintQueueItem>();
        }

        protected bool ShowDialog { get; set; }

        [Inject]
        protected HttpClient HttpClient
        {
            get; set;
        }

        [Inject]
        protected ILogger<PrinterQueueDialog> Logger
        {
            get; set;
        }

        private IEnumerable<PrintQueueItem> PrinterQueue
        {
            get;
            set;
        }

        public void Show()
        {
            ShowDialog = true;
            FetchPrinterQueue();
            StateHasChanged();
        }

        private void FetchPrinterQueue()
        {
            Task.Run(async() =>
            {
                try
                {
                    PrinterQueue = await HttpClient.GetFromJsonAsync<List<PrintQueueItem>>("api/Printer/PrinterQueue");
                    StateHasChanged();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed fetch printer queue items");
                }
            });
        }

        protected void OnCloseDialog()
        {
            ShowDialog = false;
        }

        private async Task ClearPrinterQueue()
        {

            try
            {
                await HttpClient.PostAsJsonAsync("api/Printer/ClearPrintQueue", string.Empty);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to clear print queue");
            }

            FetchPrinterQueue();
        }
    }
}
