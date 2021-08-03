using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace PhotoBooth.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AboutController : ControllerBase
    {
        [HttpGet]
        [ActionName(nameof(Version))]
        public string Version()
        {
            return Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString();
        }
    }
}
