using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace PhotoBooth.Client.Shared
{
    public partial class InfoDialog : ComponentBase
    {
        protected bool ShowConfirmation { get; set; }

        [Parameter]
        public string Title { get; set; }

        [Parameter]
        public bool ShowNegativeButton { get; set; } = false;

        [Parameter]
        public string NegativeButtonText { get; set; }

        [Parameter]
        public string PositiveButtonText
        {
            get; set;
        }

        [Parameter]
        public string Description { get; set; }

        public void Show()
        {
            ShowConfirmation = true;
            StateHasChanged();
        }

        public void Hide()
        {
            ShowConfirmation = false;
            StateHasChanged();
        }

        [Parameter]
        public EventCallback<bool> ConfirmationChanged { get; set; }

        protected async Task OnConfirmationChange(bool value)
        {
            ShowConfirmation = false;
            await ConfirmationChanged.InvokeAsync(value);
        }
    }
}
