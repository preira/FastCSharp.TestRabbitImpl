
using FastCSharp.Publisher;
using FastCSharp.RabbitPublisher.Common;
using FastCSharp.RabbitPublisher.Impl;
using FastCSharp.RabbitPublisher.Injection;

namespace FastCSharp.RabbitPublisher.Test;

public class Message
{
    private string? message;
    static int _id = 0;
    int id;
    public string? Text { get => $"{message} - {id}"; set => message = value; }
    public Message()
    {
        id = Interlocked.Increment(ref _id);
    }
}

public class Endpoints
{
    readonly RabbitConnectionPool connectionPool;
    readonly ILoggerFactory loggerFactory;
    RabbitPublisherConfig publisherConfig;
    bool batch;
    public Endpoints(ILoggerFactory loggerFactory, string configFile, bool batch = false)
    {
        this.batch = batch;
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile(configFile, true, true)
            .Build();
        var section = configuration.GetSection(RabbitOptions.SectionName);
        RabbitOptions options = new();
        section.Bind(options.Value);

        this.publisherConfig = options.Value;
        this.loggerFactory = loggerFactory;
        connectionPool = new RabbitConnectionPool(publisherConfig, loggerFactory);
    }

    public WebApplication Register(WebApplication app, string context, string displayName)
    {
        app.MapGet($"{context}/Direct/SendMessage", DirectPublish)
            .WithName($"Direct for {context}")
            .WithDisplayName(displayName);

        // http://localhost:5106/Topic/topic.1/SendMessage?message=Hello%20World
        app.MapGet($"{context}/Topic/topic.1/SendMessage", Topic1Publish)
            .WithName($"Topic for {context} topic.1")
            .WithDisplayName(displayName);

        // http://localhost:5106/Topic/topic.2/SendMessage?message=Hello%20World
        app.MapGet($"{context}/Topic/topic.2/SendMessage", Topic2Publish)
            .WithName($"Topic for {context} topic.2")
            .WithDisplayName(displayName);

        // http://localhost:5106/Fanout/SendMessage?message=Hello%20World
        app.MapGet($"{context}/Fanout/SendMessage", FanoutPublish)
            .WithName($"Fanout for {context}")
            .WithDisplayName(displayName);

        return app;
    }

    async Task<IResult> DirectPublish(string? message)
    {
        IPublisher<Message> publisher = new RabbitPublisher<Message>(connectionPool, loggerFactory, publisherConfig);
        publisher.ForExchange("DIRECT_EXCHANGE").ForQueue("TEST_QUEUE");
        return await Publish(message, publisher);
    }

    async Task<IResult> Topic1Publish(string? message)
    {
        IPublisher<Message> publisher = new RabbitPublisher<Message>(connectionPool, loggerFactory, publisherConfig);
        publisher.ForExchange("TOPIC_EXCHANGE").ForRouting("topic.1");
        return await Publish(message, publisher);
    }

    async Task<IResult> Topic2Publish(string? message)
    {
        IPublisher<Message> publisher = new RabbitPublisher<Message>(connectionPool, loggerFactory, publisherConfig);
        publisher.ForExchange("TOPIC_EXCHANGE").ForRouting("topic.2");
        return await Publish(message, publisher);
    }

    async Task<IResult> FanoutPublish(string? message)
    {
        IPublisher<Message> publisher = new RabbitPublisher<Message>(connectionPool, loggerFactory, publisherConfig);
        publisher.ForExchange("FANOUT_EXCHANGE");
        return await Publish(message, publisher);
    }

    async Task<IResult> Publish(string? message, IPublisher<Message> publisher)
    {
        if (batch)
            return await BatchPublish(message, publisher);
        
        return await SinglePublish(message, publisher);
    }

    async Task<IResult> SinglePublish(string? message, IPublisher<Message> publisher)
    {
        var m = new Message();
        m.Text = message;
        await publisher.Publish(m);
        return TypedResults.Accepted("");
    }
    
    async Task<IResult> BatchPublish(string? message, IPublisher<Message> publisher)
    {
        var msgArray = message?.Split(";");
        if (msgArray?.Length > 0)
        {
            var msgs = new List<Message>();
            foreach (var msg in msgArray)
            {
                var m = new Message();
                m.Text = msg;
                msgs.Add(m);
            }
            await publisher.Publish(msgs);
            return TypedResults.Accepted("");
        }
        return TypedResults.BadRequest(message);
    }
}
