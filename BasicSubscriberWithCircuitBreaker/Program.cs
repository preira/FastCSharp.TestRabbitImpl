using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using FastCSharp.RabbitSubscriber;
using FastCSharp.CircuitBreaker;

ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger("Program");
IConfiguration defaultConfiguration = new ConfigurationBuilder()
    .AddJsonFile("rabbitsettings.json", true, true)
    .Build();

// ## Configure a circuit breaker ##
var _backoff = new TimeSpan(0, 0, 0, 20, 5);
var minimalDelay = new TimeSpan(0, 0, 0, 0, 3);
var numberOfFailuresThreshold = 2;
var circuit =
    new EventDrivenCircuitBreaker(
        new ConsecutiveFailuresBreakerStrategy(numberOfFailuresThreshold, new FixedBackoff(_backoff))
    );

RemoteControl rc = new RemoteControl();

logger.LogInformation("Starting RabbitSubscribers for default vhost");
var subscriberFactory = new RabbitSubscriberFactory(defaultConfiguration, loggerFactory);
using var directSubscriber = subscriberFactory.NewSubscriber<Message>("DIRECT_QUEUE");

circuit.OnReset += (sender) =>
{
    logger.LogInformation("Circuit breaker reset");
    directSubscriber.Register(async (message) =>
    {
        logger.LogInformation($"Received {message?.Text}");
        return circuit.Wrap(() =>
            {
                logger.LogInformation($"Processing \"{message?.Text}\"");
                Task.Delay(1000).Wait();
                return rc.IsOk;
            });
    });
};

circuit.OnOpen += (sender) => { 
    logger.LogInformation("Circuit breaker open");
    directSubscriber.UnSubscribe(); 
};

// This is being done to avoid duplicating the Registration code.
circuit.Open(minimalDelay);
circuit.Close();

logger.LogInformation("> Enter 'ok' or 'ko' to determine the result of processing a message. Enter 'cancel' to cancel the backoff or 'q' to exit.");
string? line = "ok";
do 
{
    if (line == "")
    { }
    else if (line == "ok")
    {
        rc.IsOk = true;
    }
    else if (line == "ko")
    {
        rc.IsOk = false;
    }
    else if (line == "cancel")
    {
        circuit.CancelBackoff();
    }
    else
    {
        logger.LogInformation("Unknown command");
    }
    Console.Write($"$ IsOk: {rc.IsOk} >_ ");
} while ((line = Console.ReadLine()) != "q") ;

public class Message
{
    public Message()
    {
    }

    public string? Text { get; set; }
}

public class RemoteControl
{
    public bool IsOk { get; set; } = true;
}
