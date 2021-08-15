using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using PhotoBooth.Abstraction;
using PhotoBooth.Client.Models;

namespace PhotoBooth.Client.Pages
{
    public partial class Settings : ComponentBase
    {
        private string _selectedLanguage = "en";

        [Inject]
        protected HttpClient HttpClient
        {
            get; set;
        }

        [Inject]
        protected ILogger<Settings> Logger
        {
            get; set;
        }

        [Inject]
        protected IJSRuntime JsRuntime
        {
            get; set;
        }

        [Inject]
        protected NavigationManager NavigationManager
        {
            get; set;
        }

        public List<CameraInfo> Cameras
        {
            get;
            set;
        }

        public string PrinterQueue
        {
            get;
            set;
        }

        public List<Printer> Printers
        {
            get;
            set;
        }

        protected string SelectedLanguage
        {
            get
            {
                return _selectedLanguage;
            }
            set
            {
                if (value != _selectedLanguage)
                {
                    _selectedLanguage = value;
                    Task.Run(ChangeLanguage);
                }
            }
        }

        protected override async Task OnInitializedAsync()
        {
            SelectedPrinter = string.Empty;
            SelectedCamera = string.Empty;
            Printers = new List<Printer>();
            Cameras = new List<CameraInfo>();

            await FetchCurrentLanguage();
            await FetchSettings();
            await FetchAvailableCameras();
            await FetchPrinters();
            await FetchPrinterQueue();
        }

        private async Task FetchSettings()
        {
            try
            {
                SettingsDto dto = await HttpClient.GetFromJsonAsync<SettingsDto>("api/Settings/Settings");
                CaptureCountDownStepCount = dto.CaptureCountDownStepCount;
                ReviewCountDownStepCount = dto.ReviewCountDownStepCount;
                StepDownDurationInSeconds = dto.StepDownDurationInSeconds;
                ReviewImageWidth = dto.ReviewImageWidth;
                SelectedCamera = dto.SelectedCamera;
                SelectedPrinter = dto.SelectedPrinter;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to fetch settings");
            }
        }

        public double StepDownDurationInSeconds
        {
            get;
            set;
        }

        public int ReviewCountDownStepCount
        {
            get;
            set;
        }

        public int CaptureCountDownStepCount
        {
            get;
            set;
        }

        public int ReviewImageWidth
        {
            get;
            set;
        }


        public string SelectedCamera
        {
            get;
            set;
        }

        public string SelectedPrinter
        {
            get;
            set;
        }

        private async Task FetchAvailableCameras()
        {
            try
            {
                Cameras = await HttpClient.GetFromJsonAsync<List<CameraInfo>>("api/Camera/Cameras");
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
                Printers = await HttpClient.GetFromJsonAsync<List<Printer>>("api/Printer/Printers");
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

        protected async Task ClearPrintQueue()
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

        protected List<LanguageConfiguration> langCodes = new List<LanguageConfiguration>()
        {
            new LanguageConfiguration
            {
                IsoCode = "en",
                Name = "English"
            },
            new LanguageConfiguration
            {
                IsoCode = "de",
                Name= "Deutsch"
            }
        };

        
        protected async Task ChangeLanguage()
        {
            await JsRuntime.InvokeAsync<string>("setLanguage", _selectedLanguage);
            NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true);
        }

        private async Task FetchCurrentLanguage()
        {
            _selectedLanguage = await JsRuntime.InvokeAsync<string>("getLanguage");
            StateHasChanged();
        }


        protected async Task SaveSettings()
        {
            try
            {
                SettingsDto dto = new SettingsDto()
                {
                    CaptureCountDownStepCount = CaptureCountDownStepCount,
                    ReviewCountDownStepCount = ReviewCountDownStepCount,
                    StepDownDurationInSeconds = StepDownDurationInSeconds,
                    ReviewImageWidth = ReviewImageWidth,
                    SelectedCamera = SelectedCamera,
                    SelectedPrinter = SelectedPrinter
                };

                await HttpClient.PostAsJsonAsync("api/Settings/SetSettings", dto);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to save settings");
            }

            await FetchSettings();
        }
    }
}
