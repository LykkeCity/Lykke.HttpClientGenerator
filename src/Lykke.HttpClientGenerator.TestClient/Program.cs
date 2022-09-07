// See https://aka.ms/new-console-template for more information

using Lykke.HttpClientGenerator;
using Lykke.HttpClientGenerator.Retries;
using Lykke.HttpClientGenerator.TestClient;
using Lykke.HttpClientGenerator.TestWebService.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        var testWebServiceClientGenerator = HttpClientGenerator
            .BuildForUrl("http://localhost:5000")
            .WithServiceName<HttpErrorResponse>("Test Web Service")
            .WithRetriesStrategy(new LoggedExponentialRetryStrategy(new ExponentialRetryStrategy()))
            .Create();

        services.AddSingleton(testWebServiceClientGenerator.Generate<IWeatherForecastApi>());
        services.AddSingleton<Worker>();
    })
    .Build();

var worker = host.Services.GetRequiredService<Worker>();
await worker.ExecuteAsync();