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
using PhotoBooth.Client.Shared;

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

        protected PrinterQueueDialog PrinterDialog
        {
            get; set;
        }

        protected CapturedImageDialog ImageDialog
        {
            get;
            set;
        }

        public string PrintServerUrl
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

            await FetchPrintServerUrl();
            await FetchCurrentLanguage();
            await FetchSettings();
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
                ReviewImageQuality = dto.ReviewImageQuality;
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

        public int ReviewImageQuality
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

        private async Task FetchPrintServerUrl()
        {
            try
            {
                PrintServerUrl = await HttpClient.GetStringAsync("api/Settings/PrintServerUrl");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to fetch printServerUrl");
            }
        }

        protected void ShowPrintQueue()
        {
            PrinterDialog.Show();
        }

        protected void ShowAvailableImages()
        {
            ImageDialog.Show();
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

        protected async Task Ok_Click(bool deleteConfirmed)
        {
            await Task.Delay(10);
        }


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
                    ReviewImageQuality = ReviewImageQuality,
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
