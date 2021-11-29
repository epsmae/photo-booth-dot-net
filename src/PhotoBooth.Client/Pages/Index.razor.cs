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
        private string _selectedStep;

        private enum StepState
        {
            UsbDevices,
            Camera,
            Printer,
            Capture,
            Print
        }

        
        private static Dictionary<StepState, string> _stepDictionary = new()
        {
            {StepState.UsbDevices, "step_usb_device"},
            {StepState.Camera, "step_camera"},
            {StepState.Printer, "step_printer"},
            {StepState.Capture, "step_capture"},
            {StepState.Print, "step_print"},
        };
        private bool _setupInProgress;

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

        public string SelectedPrinter
        {
            get;
            set;
        }

        public bool IsNextDisabled
        {
            get
            {
                return ! IsNextEnabled(SelectedStep);
            }
        }

        public bool IsSpinnerVisible
        {
            get;
            set;
        }

        public bool IsReviewImageVisible
        {
            get
            {
                return !IsSpinnerVisible && !string.IsNullOrEmpty(Image);
            }
        }

        public string Image
        {
            get; set;
        }

        private bool IsNextEnabled(string stateString)
        {
            StepState state = _stepDictionary.First(e => e.Value == stateString).Key;

            if (state == StepState.Camera)
            {
                return ! string.IsNullOrEmpty(SelectedCamera);
            }

            if (state == StepState.Printer)
            {
                return ! string.IsNullOrEmpty(SelectedPrinter) && !string.IsNullOrEmpty(SelectedCamera);
            }

            if (state == StepState.Capture)
            {
                return !string.IsNullOrEmpty(SelectedPrinter) && !string.IsNullOrEmpty(SelectedCamera) && ! string.IsNullOrEmpty(_capturedImageName);
            }

            if (state == StepState.Print)
            {
                return false;
            }

            return true;
        }

        public string SelectedStep
        {
            get
            {
                return _selectedStep;
            }
            set
            {
                if (_selectedStep != value)
                {
                    _selectedStep = value;
                    StepState state = _stepDictionary.First(e => e.Value == _selectedStep).Key;
                    if (state == StepState.Camera)
                    {
                        Task.Run(ListCameras);
                    }
                    else if (state == StepState.Printer)
                    {
                        Task.Run(ListPrinters);
                    }
                }
            }
        }

        public string SelectedCamera
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

        protected override async Task OnInitializedAsync()
        {
            SelectedStep = "step_usb_device";
            SelectedPrinter = string.Empty;
            SelectedCamera = string.Empty;
            Printers = new List<Printer>();
            Cameras = new List<CameraInfo>();
            UsbDevices = new List<string>();

            await UpdateUsbDevice();

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(Navigator.ToAbsoluteUri("capturehub"))
                .WithAutomaticReconnect(new CustomRetryPolicy())
                .Build();
        }
        private Task OnSelectedStepChanged(string selectedStep)
        {
            if (IsSpinnerVisible)
            {
                return Task.CompletedTask;
            }


            int index = (int) _stepDictionary.First(e => e.Value == selectedStep).Key;

            if (index > 0)
            {
                string name = _stepDictionary[(StepState) index - 1];
                if (IsNextEnabled(name))
                {
                    SelectedStep = selectedStep;
                }
            }
            else
            {
                SelectedStep = selectedStep;
            }

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
            finally
            {
                if (Cameras != null && Cameras.Any())
                {
                    SelectedCamera = Cameras.First().CameraModel;
                }
                else
                {
                    SelectedCamera = string.Empty;
                }
                StateHasChanged();
            }
        }

        private async Task CaptureImage()
        {
            try
            {
                IsSpinnerVisible = true;
                Image = string.Empty;
                _capturedImageName = string.Empty;
                StateHasChanged();
                await EnsureHubConnected();
                PreviewCaptureResult result = await _hubConnection.InvokeAsync<PreviewCaptureResult>("CaptureImageData", SelectedCamera);
                _capturedImageName = result.FileName;
                Image = Convert.ToBase64String(result.ThumbnailData);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed capture image");
            }
            finally
            {
                IsSpinnerVisible = false;
                StateHasChanged();
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
                await _hubConnection.InvokeAsync<string>("PrintImage", SelectedPrinter, _capturedImageName);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to print image");
            }
        }


        private async Task UpdateUsbDevice()
        {
            try
            {
                UsbDevices =  await HttpClient.GetFromJsonAsync<List<string>>("api/Setup/GetUsbDevices");
            }
            catch (Exception e)
            {
                UsbDevices = new List<string>();
            }
        }

        private Task GoToNextStep()
        {
            
            int index =  (int)_stepDictionary.First(e => e.Value == SelectedStep).Key;

            if (index < _stepDictionary.Count -1)
            {
                SelectedStep = _stepDictionary[(StepState) index + 1];
                StateHasChanged();
            }

            return Task.CompletedTask;
        }

        private Task StartSetup()
        {
            _setupInProgress = true;
            StateHasChanged();
            return Task.CompletedTask;
        }

        private Task GoToPreviousStep()
        {

            int index = (int) _stepDictionary.First(e => e.Value == SelectedStep).Key;

            if (index > 0)
            {
                SelectedStep = _stepDictionary[(StepState) index -1];
                StateHasChanged();
            }

            return Task.CompletedTask;
        }


        public List<string> UsbDevices
        {
            get;
            set;
        }

        public bool SetupInProgress
        {
            get
            {
                return _setupInProgress;
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
            finally
            {
                if (Printers != null && Printers.Any())
                {
                    SelectedPrinter = Printers.First().Name;
                }
                else
                {
                    SelectedCamera = string.Empty;
                }
                StateHasChanged();
            }
        }

        protected async Task CompleteSetup()
        {
            try
            {
                SettingsDto dto = await HttpClient.GetFromJsonAsync<SettingsDto>("api/Settings/Settings");
                dto.SelectedCamera = SelectedCamera;
                dto.SelectedPrinter = SelectedPrinter;
                await HttpClient.PostAsJsonAsync("api/Settings/SetSettings", dto);
                Navigator.NavigateTo(Navigator.ToAbsoluteUri("capture").ToString(), false);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to save settings");
            }
        }
    }
}
