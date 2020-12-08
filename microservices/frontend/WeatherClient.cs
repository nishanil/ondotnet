using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace frontend
{
   public class WeatherClient
   {
      private readonly JsonSerializerOptions options = new JsonSerializerOptions()
      {
         PropertyNameCaseInsensitive = true,
         PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      };

      private readonly HttpClient client;
   private readonly ILogger<WeatherClient> _logger;
      public WeatherClient(HttpClient client, ILogger<WeatherClient> logger)
      {
         this.client = client;
         this._logger = logger;
      }

      public async Task<WeatherForecast[]> GetWeatherAsync()
      {
         try {
            var responseMessage = await this.client.GetAsync("/weatherforecast");
            
            if(responseMessage!=null)
            {
               var stream = await responseMessage.Content.ReadAsStreamAsync();
               return await JsonSerializer.DeserializeAsync<WeatherForecast[]>(stream, options);
            }
         }
         catch(HttpRequestException ex)
         {
            _logger.LogError(ex.Message);
            throw;
         }
         return new WeatherForecast[] {};

      }
   }
}