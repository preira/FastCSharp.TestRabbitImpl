using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using FastCSharp.RabbitSubscriber;

IConfiguration defaultConfiguration = new ConfigurationBuilder()
    .AddJsonFile("rabbitsettings.json", true, true)
    .Build();
ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());


var subscriberFactory = new RabbitSubscriberFactory(defaultConfiguration, loggerFactory);
using var directSubscriber = subscriberFactory.NewSubscriber<Message>("DIRECT_QUEUE");
directSubscriber.Register(async (message) =>
{
    Console.WriteLine($"Received {message?.Text}");
    return await Task.Run<bool>(()=>true);
});

using var topicSubscriber1 = subscriberFactory.NewSubscriber<Message>("TOPIC_QUEUE.1");
topicSubscriber1.Register(async (message) =>
{
    Console.WriteLine($"Received {message?.Text}");
    return await Task.Run<bool>(()=>true);
});

using var topicSubscriber2 = subscriberFactory.NewSubscriber<Message>("TOPIC_QUEUE.2");
topicSubscriber2.Register(async (message) =>
{
    Console.WriteLine($"Received {message?.Text}");
    return await Task.Run<bool>(()=>true);
});

using var fanoutSubscriber1 = subscriberFactory.NewSubscriber<Message>("FANOUT_QUEUE.1");
fanoutSubscriber1.Register(async (message) =>
{
    Console.WriteLine($"Received {message?.Text}");
    return await Task.Run<bool>(()=>true);
});

using var fanoutSubscriber2 = subscriberFactory.NewSubscriber<Message>("FANOUT_QUEUE.2");
fanoutSubscriber2.Register(async (message) =>
{
    Console.WriteLine($"Received {message?.Text}");
    return await Task.Run<bool>(()=>true);
});

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();


public class Message
{
    public Message()
    {
    }

    public string? Text { get; set; }
}
