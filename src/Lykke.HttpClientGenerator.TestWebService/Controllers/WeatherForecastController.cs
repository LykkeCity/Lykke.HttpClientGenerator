using Lykke.HttpClientGenerator.TestWebService.Contracts;
using Microsoft.AspNetCore.Mvc;
using Refit;

namespace Lykke.HttpClientGenerator.TestWebService.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase, IWeatherForecastApi
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

    [HttpGet]
    public Task<IEnumerable<WeatherForecast>> Get()
    {
        return Task.FromResult(Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
        );
    }

    [HttpGet("500")]
    public Task Get500()
    {
        throw new Exception("Hello Exception!");
    }
    
    [HttpGet("api-response-500")]
    public Task<ApiResponse<string>> GetApiResponse500()
    {
        throw new Exception("Hello Exception!");
    }
}