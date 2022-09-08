using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;

namespace Lykke.HttpClientGenerator.TestWebService.Contracts
{
    public interface IWeatherForecastApi
    {
        [Get("/WeatherForecast")]
        Task<IEnumerable<WeatherForecast>> Get();
    
        [Get("/WeatherForecast/500")]
        Task Get500();

        [Get("/WeatherForecast/api-response-500")]
        Task<ApiResponse<string>> GetApiResponse500();
    }
}