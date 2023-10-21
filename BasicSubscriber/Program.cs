using FastCSharp.CircuitBreaker;
using FastCSharp.RabbitSubscriber;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger("Program");
IConfiguration defaultConfiguration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", true, true)
    .Build();

var circuit = new EventDrivenCircuitBreaker(
    new ConsecutiveFailuresBreakerStrategy(
        5, 
        new FixedBackoff(new TimeSpan(0, 0, 0, 5))));

var subscriberFactory = new RabbitSubscriberFactory(defaultConfiguration, loggerFactory);
using var directSubscriber = subscriberFactory.NewSubscriber<string>("DIRECT_QUEUE");

circuit.OnOpen += (sender) =>
{
    logger.LogInformation("Circuit is open");
    directSubscriber.UnSubscribe();
};
circuit.OnReset += (sender) =>
{
    logger.LogInformation("Circuit is reset");
    directSubscriber.Reset();
};

directSubscriber.Register(async (message) =>
{
    try
    {
        return await circuit.WrapAsync(async () =>
        {
            logger.LogInformation($"Received {message}");
            return await Task.FromResult(true);
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error");
        return false;
    }
});

logger.LogInformation(" Press [enter] to exit.");
Console.ReadLine();
