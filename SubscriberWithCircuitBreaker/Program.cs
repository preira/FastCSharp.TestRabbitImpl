using FastCSharp.CircuitBreaker;
using FastCSharp.RabbitSubscriber;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger("Program");
var printHelpMessage = () => Console.WriteLine($"> Press [enter] to change from {RemoteControl.Status} to {RemoteControl.OtherStatus} and thus promote circuit to open or to close, or type 'q' to exit.\n");

IConfiguration defaultConfiguration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", true, true)
    .Build();

// Encapsulate the following into a builder method returning a subscriber.
var subscriberFactory = new RabbitSubscriberFactory(defaultConfiguration, loggerFactory);
using var subscriber = subscriberFactory.NewSubscriber<string>("DIRECT_QUEUE");


var builder = CircuitBreakerFactory.CreateBuilder<string?, bool>();
builder
    .Set(loggerFactory)
    .Set(subscriber.Options)
    .Set(
    async (message) =>
    {
        logger.LogInformation($"Received '{message}'.");
        if (RemoteControl.Fail)
            throw new CircuitException($"Remote control is set to fail (RemoteControl.Fail = {RemoteControl.Fail}).");
        logger.LogInformation($"'{message}' message processed.");
        printHelpMessage();
        return await Task.FromResult(!RemoteControl.Fail);
            }
    )
    .OnOpen(
    (sender) =>
    {
        logger.LogInformation("Circuit is open");
            subscriber.UnSubscribe();
        printHelpMessage();
        }
    )
    .OnClose(
    (sender) =>
    {
        logger.LogInformation("Circuit is reset");
            subscriber.Reset();
        printHelpMessage();
    }
    )
    .Build();

subscriber.Register(async (message) =>
{
    try
    {
        return await builder.WrappedCircuit(message);
    }
    catch (Exception)
    {
        logger.LogError("Error Processing message.");
        printHelpMessage();
        return false;
    }
});

CancellationTokenSource cts = new CancellationTokenSource();
// This way it will run in a thread from the thread pool.
var watchDogTask = Task.Run(() => {
    while (!cts.IsCancellationRequested)
    {
        if(!subscriber.IsHealthy)
        {
            subscriber.Reset();
        }
        Task.Delay(5000).Wait();
    }
});


// Demo control

string? input = null;
do
{
    var result = RemoteControl.Fail ? "failure" : "success";
    Console.WriteLine($"Message process result = {result}");
    printHelpMessage();

    input = Console.ReadLine();
    RemoteControl.Fail = !RemoteControl.Fail;
} while ("q" != input);

cts.Cancel();
await watchDogTask;
logger.LogInformation(" PEACEFULY EXITING.");

public class RemoteControl
{
    public static bool Fail { get; set; } = false;
    public static string Status { get => Fail ? "failure" : "success"; }
    public static string OtherStatus { get => !Fail ? "failure" : "success"; }
}