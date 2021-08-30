using Microsoft.AspNetCore.Components;
using PhotoBooth.Client.Shared;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using PhotoBooth.Abstraction;
using PhotoBooth.Abstraction.Exceptions;
using PhotoBooth.Client.Models;

namespace PhotoBooth.Client.Pages
{
    public partial class Capture : ComponentBase, IAsyncDisposable
    {
        private string _lastError;
        private HubConnection _hubConnection;
        private const string ServerNotReachableError = "Server not reachable!";

        [Inject]
        protected HttpClient HttpClient { get; set; }

        [Inject]
        protected NavigationManager Navigator
        {
            get; set;
        }

        [Inject]
        protected ILogger<Capture> Logger
        {
            get; set;
        }

        [Inject]
        protected IStringLocalizer<Index> Localizer
        {
            get;
            set;
        }

        [Inject]
        protected NavigationManager NavigationManager
        {
            get; set;
        }

        protected CaptureProcessState State
        {
            get;
            set;
        }

        protected int CountDownStep
        {
            get;
            set;
        }

        protected int ReviewCountDownStep
        {
            get;
            set;
        }

        protected string Image
        {
            get;
            set;
        }

        protected InfoDialog InfoDialog { get; set; }

        protected string LastError
        {
            get
            {
                return _lastError;
            }
        }

        protected bool IsCaptureButtonVisible
        {
            get
            {
                return State == CaptureProcessState.Ready;
            }
        }

        protected bool IsPrintButtonVisible
        {
            get
            {
                return State == CaptureProcessState.Review && ! string.IsNullOrEmpty(Image);
            }
        }

        protected bool IsCountDownVisible
        {
            get
            {
                return State == CaptureProcessState.CountDown;
            }
        }

        protected bool IsReviewCountDownVisible
        {
            get
            {
                return State == CaptureProcessState.Review && ReviewCountDownStep > 0;
            }
        }


        protected bool IsReviewImageVisible
        {
            get
            {
                return State == CaptureProcessState.Review && ! string.IsNullOrEmpty(Image);
            }
        }

        protected bool IsSpinnerVisible
        {
            get
            {
                return State == CaptureProcessState.Capture || State ==CaptureProcessState.Print || (State == CaptureProcessState.Review && string.IsNullOrEmpty(Image));
            }
        }


        protected override async Task OnInitializedAsync()
        {
            Logger.LogInformation("Setup hub connection");

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(Navigator.ToAbsoluteUri("capturehub"))
                .WithAutomaticReconnect(new CustomRetryPolicy())
                .Build();

            _hubConnection.On<CaptureProcessState>("ReceiveStateChanged", (state) =>
            {
                State = state;
                HandleStateUpdate();
            });


            _hubConnection.On<int>("ReceiveReviewCountDownStepChanged", (step) =>
            {
                ReviewCountDownStep = step;
                StateHasChanged();
            });

            _hubConnection.On<int>("ReceiveCountDownStepChanged", (step) =>
            {
                CountDownStep = step;
                StateHasChanged();
            });

            try
            {
                await _hubConnection.StartAsync();
                await UpdateServerState();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unable to connect to server");
                State = CaptureProcessState.Error;
                _lastError = ServerNotReachableError;
                InfoDialog.Show();
            }
        }

        private void HandleStateUpdate()
        {
            if (State == CaptureProcessState.Review)
            {
                Task.Run(async () =>
                {
                    await UpdateImage();
                    StateHasChanged();
                });
            }
            else
            {
                Image = string.Empty;
            }

            if (State == CaptureProcessState.Error)
            {
                Task.Run(async () =>
                {
                    await UpdateErrorState();
                    StateHasChanged();
                });
            }

            StateHasChanged();
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.DisposeAsync();
            }
        }
        
        protected async Task ConfirmDelete_Click(bool deleteConfirmed)
        {

            if (_lastError == ServerNotReachableError)
            {
                _lastError = string.Empty;
                State = default;

                NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true);
            }
            else
            {
                try
                {
                    await HttpClient.PostAsJsonAsync("api/Capture/ConfirmError", string.Empty);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to confirm error");
                }
            }
        }
        
        protected async Task CaptureImage()
        {
            try
            {
                await HttpClient.PostAsJsonAsync("api/Capture/Capture", string.Empty);
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to capture image");
            }
        }

        protected async Task PrintImage()
        {
            try
            {
                await HttpClient.PostAsJsonAsync("api/Capture/Print", string.Empty);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to print image");
            }
        }

        private async Task UpdateErrorState()
        {

            try
            {
                CaptureError lastErrorException = await HttpClient.GetFromJsonAsync<CaptureError>("api/Capture/LastException");
                _lastError = TryGetLocalizedErrorMessage(lastErrorException);

                if (State == CaptureProcessState.Error && ! string.IsNullOrEmpty(_lastError))
                {
                    InfoDialog.Show();
                }
            }
            catch (Exception ex)
            {
                _lastError = string.Empty;
                Logger.LogError(ex, "Failed to fetch error");
            }
        }

        private string TryGetLocalizedErrorMessage(CaptureError error)
        {
            if (error.Exception == PhotoBoothExceptions.NoPrinterAvailable)
            {
                return Localizer.GetString("capture.error.no_camera_available");
            }

            if (error.Exception == PhotoBoothExceptions.GeneralPrinterError)
            {
                return Localizer.GetString("capture.error.printer_error");
            }

            if (error.Exception == PhotoBoothExceptions.NoCameraAvailable)
            {
                return Localizer.GetString("capture.error.no_printer_available");
            }

            if (error.Exception == PhotoBoothExceptions.CameraOutOfFocus)
            {
                return Localizer.GetString("capture.error.camera_out_of_focus");
            }

            if (error.Exception == PhotoBoothExceptions.GeneralCaptureError)
            {
                return Localizer.GetString("capture.error.capture_error");
            }

            if (error.Exception == PhotoBoothExceptions.CameraClaimFailed)
            {
                return Localizer.GetString("capture.error.camera_claim_failed");
            }


            return error != null ? error.ErrorMessage :  string.Empty;
        }

        private async Task UpdateServerState()
        {
            try
            {
                State = await HttpClient.GetFromJsonAsync<CaptureProcessState>("api/Capture/State");
                HandleStateUpdate();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to update server state");
            }
        }

        private async Task UpdateImage()
        {
            try
            {
                Logger.LogInformation("Loading image from server");
                byte[] imageData = await HttpClient.GetFromJsonAsync<byte[]>("api/Capture/ImageData");

                if (imageData == null)
                {
                    Image = string.Empty;
                }
                else
                {
                    Image = Convert.ToBase64String(imageData);
                }
                Logger.LogInformation($"Loaded image length={Image?.Length}");
            }

            catch (Exception ex)
            {
                Image = string.Empty;
                Logger.LogError(ex, $"Failed to load image");
            }
        }
    }
}
