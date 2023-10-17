using System.Collections.Concurrent;
using System.Diagnostics;
using FastCSharp.Pool;
using FastCSharp.Publisher;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var services = builder.Services;
services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo {
        Title = "Fast Framework.Test API",
        Description = "Test API",
        Version = "v0" });
    c.TagActionsBy(d =>
    {
        return new List<string>() { d.ActionDescriptor.DisplayName! };
    });
});

// services.AddRabbitPublisher("rabbitsettings.CLUSTER.json");
services.AddRabbitPublisher("rabbitsettings.json");

var app = builder.Build();
app.UseRouting();
// http://localhost:5106/swagger/v1/swagger.json
app.UseSwagger();
// http://localhost:5106/swagger
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fast Framework.Test API");
});
var context = "fastcsharp";
var displayName = "Default VHost";
app.MapGet($"{context}/Direct/SendMessage", async (string message, IPublisherFactory<IDirectPublisher> publisherFactory) => 
{
    using var publisher = publisherFactory.NewPublisher<string>("DIRECT_EXCHANGE", "TEST_QUEUE");
    var result = await publisher.Publish(message);
    if(result)
    {
        // Console.WriteLine($"Message {message} sent to {publisher.GetType().Name}");
        return Results.Ok("Success");
    }
    else 
    {
        // Console.WriteLine($"Error publishing Message {message} sent to {publisher.GetType().Name}");
        return Results.Problem("Error");
    }
})
    .WithName($"Direct for {context}")
    .WithDisplayName(displayName);

// http://localhost:5106/Topic/topic.1/SendMessage?message=Hello%20World
app.MapGet($"{context}/Topic/topic.q.1/SendMessage", async (string message, IPublisherFactory<ITopicPublisher> publisherFactory) => 
{
    using var publisher = publisherFactory.NewPublisher<string>("TOPIC_EXCHANGE", "test.topic.q.1");
    var result = await publisher.Publish(message);
    if(result)
    {
        // Console.WriteLine($"Message {message} sent to {publisher.GetType().Name}");
        return Results.Ok("Success");
    }
    else 
    {
        // Console.WriteLine($"Error publishing Message {message} sent to {publisher.GetType().Name}");
        return Results.Problem("Error");
    }
})
    .WithName($"Topic for {context} topic.1")
    .WithDisplayName(displayName);

// http://localhost:5106/Topic/topic.2/SendMessage?message=Hello%20World
app.MapGet($"{context}/Topic/topic.q.2/SendMessage", async (string message, IPublisherFactory<ITopicPublisher> publisherFactory) => 
{
    using var publisher = publisherFactory.NewPublisher<string>("TOPIC_EXCHANGE", "test.topic.q.2");
    var result = await publisher.Publish(message);
    if(result)
    {
        // Console.WriteLine($"Message {message} sent to {publisher.GetType().Name}");
        return Results.Ok("Success");
    }
    else 
    {
        // Console.WriteLine($"Error publishing Message {message} sent to {publisher.GetType().Name}");
        return Results.Problem("Error");
    }
})
    .WithName($"Topic for {context} topic.2")
    .WithDisplayName(displayName);

// http://localhost:5106/Fanout/SendMessage?message=Hello%20World
app.MapGet($"{context}/Fanout/SendMessage", async (string message, IPublisherFactory<IFanoutPublisher> publisherFactory) => 
{
    var publisher = publisherFactory.NewPublisher<string>("FANOUT_EXCHANGE");
    var result = await publisher.Publish(message);
    if(result)
    {
        // Console.WriteLine($"Message {message} sent to {publisher.GetType().Name}");
        return Results.Ok("Success");
    }
    else 
    {
        // Console.WriteLine($"Error publishing Message {message} sent to {publisher.GetType().Name}");
        return Results.Problem("Error");
    }
})
    .WithName($"Fanout for {context}")
    .WithDisplayName(displayName);

app.MapPost($"{context}/Fantout/SendMessage", (
        LoadRequest request, 
        IPublisherFactory<IFanoutPublisher> fanoutFactory) =>
{
    var publisher = fanoutFactory.NewPublisher<Message>("FANOUT_EXCHANGE");
    var msgArray = request.Message?.Split(";");
    var msgs = msgArray?.Select(m => new Message { Text = m });
    msgs ??= new List<Message> { new() { Text = "Hello World" } };
    return Send(request, msgs.Count(), async () => {
        return await publisher.Publish(msgs.First());
    });
});

app.MapPost($"{context}/Fantout/SendBatch", (
        LoadRequest request, 
        IBatchPublisherFactory<IFanoutPublisher> fanoutFactory) =>
{
    var publisher = fanoutFactory.NewPublisher<Message>("FANOUT_EXCHANGE");
    var msgArray = request.Message?.Split(";");
    var msgs = msgArray?.Select(m => new Message { Text = m });
    msgs ??= new List<Message> { new() { Text = "Hello World" } };
    return Send(request, msgs.Count(), async () => {
        return await publisher.Publish(msgs);
    });
});

app.MapPost($"{context}/Load/SendMessage", (
        LoadRequest request, 
        IPublisherFactory<IFanoutPublisher> fanoutFactory,
        IPublisherFactory<ITopicPublisher> topicFactory,
        IPublisherFactory<IDirectPublisher> directFactory,
        IBatchPublisherFactory<IFanoutPublisher> batchFanoutFactory,
        IBatchPublisherFactory<ITopicPublisher> batchTopicFactory,
        IBatchPublisherFactory<IDirectPublisher> batchDirectFactory
    ) =>
{
    var msgArray = request.Message?.Split(";");
    var msgs = msgArray?.Select(m => new Message { Text = m });
    msgs ??= new List<Message> { new() { Text = "Hello World" } };

    if(request.IsBatch)
    {
        return Send(request, msgs.Count(), async () => {
            using IBatchPublisher<Message>? publisher = request.ExchangeType switch
            {
                "direct" => batchDirectFactory.NewPublisher<Message>("DIRECT_EXCHANGE", "TEST_QUEUE"),
                "topic" => batchTopicFactory.NewPublisher<Message>("TOPIC_EXCHANGE", "topic.1"),
                "fanout" => batchFanoutFactory.NewPublisher<Message>("FANOUT_EXCHANGE"),
                _ => null,
            } ?? throw new Exception($"Publisher not found for {request.ExchangeType}");
            return await publisher.Publish(msgs);
        });
    }
    else
    {
        return Send(request, msgs.Count(), async () => {
            using IPublisher<Message>? publisher = request.ExchangeType switch
            {
                "direct" => directFactory.NewPublisher<Message>("DIRECT_EXCHANGE", "TEST_QUEUE"),
                "topic" => topicFactory.NewPublisher<Message>("TOPIC_EXCHANGE", "topic.1"),
                "fanout" => fanoutFactory.NewPublisher<Message>("FANOUT_EXCHANGE"),
                _ => null,
            } ?? throw new Exception($"Publisher not found for {request.ExchangeType}");

            return await publisher.Publish(msgs.First());
        });
    }
})
    .WithName($"Single for {context}")
    .WithDisplayName(displayName);

static IResult Send(LoadRequest request, int msgCount, Func<Task<bool>> Publish)
{
    ConcurrentDictionary<int, Stats> stats = new();
    List<Thread> threads = new();

    int idx = 0;

    for (int i = 0; i < request.Threads; ++i)
    {
        var t = new Thread(() =>
        {
            Stats stat = new();
            int k = Interlocked.Increment(ref idx);
            stats.TryAdd(k, stat);

            for (int j = 0; j < request.NumberOfRequests; j++)
            {
                Stopwatch sw = new();
                sw.Start();

                string threadName = $"{Thread.CurrentThread.Name} ({Thread.CurrentThread.GetHashCode()}/{ThreadPool.ThreadCount})";

                Task<bool> task = Publish();
                task.Wait();
                
                if (task.IsCompletedSuccessfully && task.Result)
                {
                    // Console.WriteLine($"publisher {threadName} >> Message Sent!");
                    stat.Success++;
                }
                else
                {
                    // Console.WriteLine($"publisher {threadName} >> Message not sent!");
                    // Console.WriteLine(task.Exception);
                    stat.Errors++;
                }

                sw.Stop();
                stat.TotalCount += msgCount;
                stat.TotalTime += sw.Elapsed;
                if (stat.MinTime > sw.Elapsed) stat.MinTime = sw.Elapsed;
                if (stat.MaxTime < sw.Elapsed) stat.MaxTime = sw.Elapsed;
                Thread.Sleep(request.LagMillisecond);
            }
        });
        threads.Add(t);
        t.Start();
    }
    while(threads.Count > 0)
    {
        Thread t = threads.First();
        t.Join();
        threads.Remove(t);
        Console.WriteLine($"Thread {t.Name} finished");
    }
    Console.WriteLine($"ThreadPool final size {ThreadPool.ThreadCount} >> Load Test Finished!");
    var totals = stats.Aggregate(
        new Stats(),
        (acc, i) =>
        {
            acc.TotalCount += i.Value.TotalCount;
            acc.Errors += i.Value.Errors;
            acc.Success += i.Value.Success;
            if (acc.TotalTime < i.Value.TotalTime) acc.TotalTime = i.Value.TotalTime;
            if (acc.MinTime > i.Value.MinTime) acc.MinTime = i.Value.MinTime;
            if (acc.MaxTime < i.Value.MaxTime) acc.MaxTime = i.Value.MaxTime;
            return acc;
        });
    stats.TryAdd(-1, totals);
    return TypedResults.Ok(stats);
}

app.MapGet($"{context}/Pool/Statistics", (
        string publisherType, 
        bool IsBatch, 
        IPublisherFactory<IFanoutPublisher> fanoutFactory,
        IPublisherFactory<ITopicPublisher> topicFactory,
        IPublisherFactory<IDirectPublisher> directFactory,
        IBatchPublisherFactory<IFanoutPublisher> batchFanoutFactory,
        IBatchPublisherFactory<ITopicPublisher> batchTopicFactory,
        IBatchPublisherFactory<IDirectPublisher> batchDirectFactory
    ) => 
{
    IPoolStats? stats = null;
    switch (publisherType)
    {
        case "direct":
            stats = IsBatch ? batchDirectFactory.PoolStats : directFactory.PoolStats;
            break;
        case "topic":
            stats = IsBatch ? batchTopicFactory.PoolStats : topicFactory.PoolStats;
            break;
        case "fanout":
            stats = IsBatch ? batchFanoutFactory.PoolStats : fanoutFactory.PoolStats;
            break;
        default:
            return Results.Problem("Publisher not found");
    }
    return TypedResults.Ok(stats);
})
    .WithName($"Pool Stats")
    .WithDisplayName("Stats");


app.Run();

public class LoadRequest
{
    // public string? VHost { get; set; }
    public string? Message { get; set; }
    public string? ExchangeType { get; set; }
    public bool IsBatch { get; set; }
    public int Threads { get; set; }
    public int LagMillisecond { get; set; }
    public int NumberOfRequests { get; set; }
}

public class Message
{
    public Message()
    {
    }

    public string? Text { get; set; }
}

public class Stats
{
    public int TotalCount { get; set; }
    public int Errors { get; set; }
    public int Success { get; set; }
    public TimeSpan TotalTime { get; set; }
    public TimeSpan MinTime { get; set; }
    public TimeSpan MaxTime { get; set; }
    public TimeSpan AvgTime { get { return TimeSpan.FromMilliseconds(TotalCount == 0 ? 0 : TotalTime.TotalMilliseconds / TotalCount); } }
    public int Throughput { get { return (int)(TotalCount == 0 ? 0 : TotalCount / TotalTime.TotalSeconds); } }
    public decimal ErrorRate { get { return TotalCount != 0 ? (decimal)100 * Errors / TotalCount : 0; } }
    public Stats()
    {
        Errors = 0;
        Success = 0;
        TotalCount = 0;
        MinTime = TimeSpan.MaxValue;
        MaxTime = TimeSpan.MinValue;
        TotalTime = TimeSpan.Zero;
    }
}

