using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace backend.Controllers
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

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        //  [HttpGet]
        //  public IEnumerable<WeatherForecast> Get()
        //  {
        //      var rng = new Random();
        //      return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        //      {
        //          Date = DateTime.Now.AddDays(index),
        //          TemperatureC = rng.Next(-20, 55),
        //          Summary = Summaries[rng.Next(Summaries.Length)]
        //      })
        //      .ToArray();
        //  }

        [HttpGet]
        public async Task<string> Get([FromServices] IDistributedCache cache)
        {
            var weather = await cache.GetStringAsync("weather");

            if (weather == null)
            {
                var rng = new Random();
                var forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = Summaries[rng.Next(Summaries.Length)]
                })
                .ToArray();

                weather = JsonSerializer.Serialize(forecasts);

                await cache.SetStringAsync("weather", weather, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5)
                });
            }
            return weather;
        }
    }
}
