using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using FastCSharp.RabbitSubscriber;

ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger("Program");
IConfiguration defaultConfiguration = new ConfigurationBuilder()
    .AddJsonFile("rabbitsettings.CLUSTER.json", true, true)
    .Build();

logger.LogInformation("Starting RabbitSubscribers for default vhost");
var subscriberFactory = new RabbitSubscriberFactory(defaultConfiguration, loggerFactory);
using var directSubscriber = subscriberFactory.NewSubscriber<Message>("DIRECT_QUEUE");
directSubscriber.Register(async (message) =>
{
    logger.LogInformation($"Received {message?.Text}");
    return await Task.Run<bool>(()=>true);
});

using var topicSubscriber1 = subscriberFactory.NewSubscriber<Message>("TOPIC_QUEUE.1");
topicSubscriber1.Register(async (message) =>
{
    logger.LogInformation($"Received {message?.Text}");
    return await Task.Run<bool>(()=>true);
});

using var topicSubscriber2 = subscriberFactory.NewSubscriber<Message>("TOPIC_QUEUE.2");
topicSubscriber2.Register(async (message) =>
{
    logger.LogInformation($"Received {message?.Text}");
    return await Task.Run<bool>(()=>true);
});

using var fanoutSubscriber1 = subscriberFactory.NewSubscriber<Message>("FANOUT_QUEUE.1");
fanoutSubscriber1.Register(async (message) =>
{
    logger.LogInformation($"Received {message?.Text}");
    return await Task.Run<bool>(()=>true);
});

using var fanoutSubscriber2 = subscriberFactory.NewSubscriber<Message>("FANOUT_QUEUE.2");
fanoutSubscriber2.Register(async (message) =>
{
    logger.LogInformation($"Received {message?.Text}");
    return await Task.Run<bool>(()=>true);
});


logger.LogInformation("Starting RabbitSubscribers for vhost test-vhost");
IConfiguration vhostConfiguration = new ConfigurationBuilder()
    .AddJsonFile("rabbitsettings.VHOST.json", true, true)
    .Build();

var vHostSubscriberFactory = new RabbitSubscriberFactory(vhostConfiguration, loggerFactory);
using var vHostDirectSubscriber = vHostSubscriberFactory.NewSubscriber<Message>("DIRECT_QUEUE");
vHostDirectSubscriber.Register(async (message) =>
{
    logger.LogInformation($"Received {message?.Text}");
    return await Task.Run<bool>(()=>true);
});

using var vHostTopicSubscriber1 = vHostSubscriberFactory.NewSubscriber<Message>("TOPIC_QUEUE.1");
vHostTopicSubscriber1.Register(async (message) =>
{
    logger.LogInformation($"Received {message?.Text}");
    return await Task.Run<bool>(()=>true);
});

using var vHostTopicSubscriber2 = vHostSubscriberFactory.NewSubscriber<Message>("TOPIC_QUEUE.2");
vHostTopicSubscriber2.Register(async (message) =>
{
    logger.LogInformation($"Received {message?.Text}");
    return await Task.Run<bool>(()=>true);
});

using var vHostFanoutSubscriber1 = vHostSubscriberFactory.NewSubscriber<Message>("FANOUT_QUEUE.1");
vHostFanoutSubscriber1.Register(async (message) =>
{
    logger.LogInformation($"Received {message?.Text}");
    return await Task.Run<bool>(()=>true);
});

using var vHostFanoutSubscriber2 = vHostSubscriberFactory.NewSubscriber<Message>("FANOUT_QUEUE.2");
vHostFanoutSubscriber2.Register(async (message) =>
{
    logger.LogInformation($"Received {message?.Text}");
    return await Task.Run<bool>(()=>true);
});

logger.LogInformation(" Press [enter] to exit.");
Console.ReadLine();


public class Message
{
    public Message()
    {
    }

    public string? Text { get; set; }
}
