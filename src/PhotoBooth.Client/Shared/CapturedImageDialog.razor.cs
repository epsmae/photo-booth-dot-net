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
    public partial class CapturedImageDialog : ComponentBase
    {

        public CapturedImageDialog()
        {
            Images = new List<FileInfo>();
        }

        protected bool ShowDialog { get; set; }

        [Inject]
        protected HttpClient HttpClient
        {
            get; set;
        }

        [Inject]
        protected ILogger<CapturedImageDialog> Logger
        {
            get; set;
        }

        private IList<FileInfo> Images
        {
            get;
            set;
        }

        public void Show()
        {
            ShowDialog = true;
            FetchAvailableImages();
            StateHasChanged();
        }

        private void FetchAvailableImages()
        {
            Task.Run(async() =>
            {
                try
                {
                    Images = await HttpClient.GetFromJsonAsync<List<FileInfo>>("api/Settings/AvailableImages");
                    StateHasChanged();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed fetch available images");
                }
            });
        }

        protected void OnCloseDialog()
        {
            ShowDialog = false;
        }

        private async Task ClearImages()
        {
            try
            {
                await HttpClient.PostAsJsonAsync("api/Settings/ClearImages", string.Empty);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to clear image directory");
            }

            FetchAvailableImages();
        }
    }
}
