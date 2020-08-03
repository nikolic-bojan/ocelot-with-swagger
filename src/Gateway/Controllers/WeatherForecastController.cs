using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace OldGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            //var client = _httpClientFactory.CreateClient("api");
            //var response = await client.GetAsync("weatherforecast");

            //var result = JsonSerializer.Deserialize<IEnumerable<WeatherForecast>>(await response.Content.ReadAsByteArrayAsync());

            //return result;

            throw new System.NotImplementedException();
        }
    }
}
