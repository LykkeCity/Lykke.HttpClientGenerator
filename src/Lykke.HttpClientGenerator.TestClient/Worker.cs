using Lykke.HttpClientGenerator.TestWebService.Contracts;

class Worker
{
    private readonly IWeatherForecastApi _weatherForecastApi;

    public Worker(IWeatherForecastApi weatherForecastApi)
    {
        _weatherForecastApi = weatherForecastApi;
    }

    public async Task ExecuteAsync()
    {
        Console.WriteLine("In ExecuteAsync");

        try
        {
            // await ExecuteGetAsync();
            // await _weatherForecastApi.Get500();
            await _weatherForecastApi.GetApiResponse500();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
        Console.WriteLine("Finished");
    }

    private async Task ExecuteGetAsync()
    {
        var result = await _weatherForecastApi.Get();
        Console.WriteLine($"Count: {result.Count()}");
    }
}