using Microsoft.AspNetCore.Components;
using PhotoBooth.Client.Shared;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using PhotoBooth.Abstraction;

namespace PhotoBooth.Client.Pages
{
    public partial class Capture : ComponentBase, IAsyncDisposable
    {
        private string _lastError;
        private HubConnection _hubConnection;

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
            _hubConnection = new HubConnectionBuilder().WithUrl(Navigator.ToAbsoluteUri("capturehub"))
                .Build();

            _hubConnection.On<CaptureProcessState>("ReceiveStateChanged", (state) =>
            {
                //var encodedMsg = $"{user}: {message}";
                //messages.Add(encodedMsg);
                State = state;

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


            await _hubConnection.StartAsync();

            await UpdateServerState();
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
            try
            {
                await HttpClient.PostAsJsonAsync("api/Capture/ConfirmError", string.Empty);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to confirm error");
            }
        }
        
        protected async Task CaptureImage()
        {
            await HttpClient.PostAsJsonAsync("api/Capture/Capture", string.Empty);
            StateHasChanged();
        }

        protected async Task PrintImage()
        {
            await HttpClient.PostAsJsonAsync("api/Capture/Print", string.Empty);
        }

        private async Task UpdateErrorState()
        {

            try
            {
                CaptureError lastErrorException = await HttpClient.GetFromJsonAsync<CaptureError>("api/Capture/LastException");
                _lastError = lastErrorException != null ? lastErrorException.ErrorMessage :  string.Empty;

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

        private async Task UpdateServerState()
        {
            try
            {
                State = await HttpClient.GetFromJsonAsync<CaptureProcessState>("api/Capture/State");
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
