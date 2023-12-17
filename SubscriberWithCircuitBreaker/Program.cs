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
using var directSubscriber = subscriberFactory.NewSubscriber<string>("DIRECT_QUEUE");

var  wrappedOnMessage = CircuitBreakerFactory.NewAsyncBreaker<string?, bool>(
    loggerFactory,
    directSubscriber.Options,
    async (message) =>
    {
        logger.LogInformation($"Received '{message}'.");
        if (RemoteControl.Fail)
            throw new CircuitException($"Remote control is set to fail (RemoteControl.Fail = {RemoteControl.Fail}).");
        logger.LogInformation($"'{message}' message processed.");
        printHelpMessage();
        return await Task.FromResult(!RemoteControl.Fail);
    },
    (sender) =>
    {
        logger.LogInformation("Circuit is open");
        directSubscriber.UnSubscribe();
        printHelpMessage();
    },
    (sender) =>
    {
        logger.LogInformation("Circuit is reset");
        directSubscriber.Reset();
        printHelpMessage();
    }
);

directSubscriber.Register(async (message) =>
{
    try
    {
        return await wrappedOnMessage(message);
    }
    catch (Exception)
    {
        logger.LogError("Error Processing message.");
        printHelpMessage();
        return false;
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
logger.LogInformation(" PEACEFULY EXITING.");

public class RemoteControl
{
    public static bool Fail { get; set; } = false;
    public static string Status { get => Fail ? "failure" : "success"; }
    public static string OtherStatus { get => !Fail ? "failure" : "success"; }
}