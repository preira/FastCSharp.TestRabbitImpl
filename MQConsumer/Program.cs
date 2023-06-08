using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using FastCSharp.RabbitSubscriber;

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("rabbitsettings.json", true, true)
    .Build();
ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());


var exchange = new RabbitSubscriberFactory(configuration, loggerFactory);
using var subscriber = exchange.NewSubscriber<Message>("TASK_QUEUE");
subscriber.Register(async (message) =>
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
