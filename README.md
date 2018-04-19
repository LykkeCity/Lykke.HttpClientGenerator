# Lykke.ClientGenerator
Helps to generate client proxy for an http api using the Refit library. It adds cache and retries using Polly to the generated proxies.

Nuget: https://www.nuget.org/packages/Lykke.ClientGenerator/

# Quickstart guide
1. Create a nuget package containing interface of your service with the data types used in it.
2. In a consumer application add reference to the interface package and to this one.
3. To create an instance of the client proxy use this code:
```csharp
var generator = ClientProxyGenerator.CreateDefault(serviceUrl);
var client = generator.Generate<IApi>();
```
This example creates a client with a linear retry strategy with default params (6 times with 5 sec pauses) 
and caching with parameters specified in method attributes.
To add caching to a method of an interface:
```csharp
[ClientCaching(Minutes = 1, Seconds = 30)]
[Get("/api/get-foo"]
Task<string> GetFoo();
```
Only get methods caching is supported via attributes by default. 

Usually you should create a single instance of each proxy in your application and reuse it everywhere. 
It is completely thread-safe. Creating multiple instances can cause some problems.

# Customizations
ClientProxyGenerator contains 3 overloads of CreateDefault static method:
- accepting only the serviceUrl
- accepting the serviceUrl and an api key as a second parameter - it is then added to all requests as the api-key header
- all the same but it has the third parameter of type IRetryStrategy - it can be used to set custom retries parameters 
or disable retries (if null is passed).

There is also a method for customization of everything else:
```csharp
IEnumerable<ICallsWrapper> callsWrappers = 
  ClientProxyGenerator.GetDefaultCallsWrappers();
IEnumerable<Func<HttpMessageHandler, HttpMessageHandler>> httpMessageHandlerProviders = 
  ClientProxyGenerator.GetDefaultHttpMessageHandlerProviders(
  new ExponentialRetryStrategy(maxRetrySleepDuration: TimeSpan.FromMinutes(2), retryAttemptsCount: 20, 
      exponentBase: Math.E), 
  "your api key");
ClientProxyGenerator.CreateCustom("http://my.service", callsWrappers, httpMessageHandlerProviders);
var client = generator.Generate<IApi>();
```

## Calls wrappers
Calls wrappers (or handlers) are executed around the api interface methods:
```csharp
public interface ICallsWrapper
{
    Task<object> HandleMethodCall(MethodInfo targetMethod, object[] args, Func<Task<object>> innerHandler);
}
```

Example wrapper implementation:
```csharp
public async Task<object> HandleMethodCall(MethodInfo targetMethod, object[] args, Func<Task<object>> innerHandler)
{
    Console.WriteLine($"{method.Name} before");
    var result = await inner();
    Console.WriteLine($"{method.Name} after");
    return result;
};
```
If the function do not call `inner()` - it effectively restricts the invocation of actual wrapped method.
One can add try-catch here, execute inner code in a separate thread, add logging, caching or retries, etc.
The wrappers are executed in the order they are passed to the CreateCustom() method. 
Calling `inner()` in the first handler means calling the second one.
Calling `inner()` in the last one - means calling the actial method.


ClientProxyGenerator.GetDefaultCallsWrappers() returns only one default wrapper: AttributeBasedCachingCallsWrapper.
It can be replaced with something else (ex. with a derived type), removed to get rid of the caching functionality.

### Caching customization
AttributeBasedCachingCallsWrapper derives from an abstract class CachingCallsWrapper. 
It has a `protected abstract TimeSpan GetCachingTime(MethodInfo targetMethod, object[] args)` method.
Overriding it one can provide some logic to get the caching time. Only get methods can be cached this way.

## HttpMessageHandlers
Other, more performant, way of adding custom logic to requests is to use HttpMessageHandlers.

They do not wrap the Refit logic of transforming a method call to an http request, instead they wrap the actual http request logic.

A factory-method style is used to specify them. Create several `Func<HttpMessageHandler, HttpMessageHandler>`. 
The argument of the function is the next handler to be executed - the inner one. 
HttpMessageHandlers are constructed using these functions in the order they are passed.
The last one receives a HttpClientHandler which makes the request.

Example: 
```csharp
var httpMessageHandlerProviders = ClientProxyGenerator.GetDefaultHttpMessageHandlerProviders(
    new LinearRetryStrategy(TimeSpan.FromSeconds(1), 1)
    .Concat(inner => new MySpecialHandler(inner));
```
```csharp
public class MySpecialHandler : DelegatingHandler
{
    public MySpecialHandler(HttpMessageHandler innerHandler) : base(innerHandler) { }
    
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Console.WriteLine("before");
        await base.SendAsync(request, cancellationToken);
        Console.WriteLine("after");
    }
}
```
There are some default handlers in the library:
- ApiKeyHeaderHttpClientHandler
- UserAgentHeaderHttpClientHandler
- RetryingHttpClientHandler

### Retries customization
The RetryingHttpClientHandler accepts a IRetryStrategy:
```csharp
public interface IRetryStrategy
{
    TimeSpan GetRetrySleepDuration(int retryAttempt, string url);
    int RetryAttemptsCount { get; }
}
```
There are 2 default configurable implementations:
- LinearRetryStrategy
- ExponentialRetryStrategy

Implementing more sophisticated retries logic may require creating a separate RetryingHttpClientHandler implementation.

