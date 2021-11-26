using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace PhotoBooth.Client.Shared
{
    public partial class StepItem : ComponentBase
    {
        [Parameter]
        public System.Func<Task> NextAction
        {
            get; set;
        }

        [Parameter]
        public System.Func<Task> PreviousAction
        {
            get; set;
        }

        [Parameter]
        public bool IsNextDisabled
        {
            get; set;
        }

        [Parameter]
        public string Header
        {
            get; set;
        }

        [Parameter]
        public string Title
        {
            get; set;
        }

        [Parameter]
        public string Description
        {
            get; set;
        }

        [Parameter]
        public RenderFragment ChildContent
        {
            get; set;
        }

        public Task GoToNextStep()
        {
            return NextAction();
        }

        public Task GoToPreviousStep()
        {
            return PreviousAction();
        }

    }
}
