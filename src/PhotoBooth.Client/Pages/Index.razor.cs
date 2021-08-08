using System.Net.Http;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

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
    }
}
