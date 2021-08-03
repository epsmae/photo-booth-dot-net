using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PhotoBooth.Abstraction;
using PhotoBooth.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoBooth.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private ICameraService _cameraService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, ICameraService cameraService)
        {
            _logger = logger;
            _cameraService = cameraService;
        }

        [HttpGet]
        public async Task<byte[]> GetImageData()
        {

            try
            {
                _logger.LogInformation($"Capturing image");
                return await _cameraService.CaptureImageData();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to trigger capture");
            }

            return new byte[] { };
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
                            
            try
            {
                _logger.LogInformation($"Capturing image");
                await _cameraService.CaptureImage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to trigger capture");
            }

            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
