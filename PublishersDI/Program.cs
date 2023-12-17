using System.Collections.Concurrent;
using System.Diagnostics;
using FastCSharp.Publisher;
using FastCSharp.RabbitPublisher.Common;
using FastCSharp.RabbitPublisher.Impl;
using Microsoft.Extensions.Options;
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
services.AddRabbitPublisher<string>("rabbitsettings.CLUSTER.json");
services.AddRabbitPublisher<Message>("rabbitsettings.CLUSTER.json");

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
app.MapGet($"{context}/Direct/SendMessage", async (string message, IPublisher<string> publisher) => 
    {
        
        var result = await publisher.ForExchange("DIRECT_EXCHANGE").ForQueue("TEST_QUEUE").Publish(message);
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
app.MapGet($"{context}/Topic/topic.q.1/SendMessage", async (string message, IPublisher<string> publisher) => 
    {
        
        var result = await publisher.ForExchange("TOPIC_EXCHANGE").ForRouting("topic.1").Publish(message);
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
app.MapGet($"{context}/Topic/topic.q.2/SendMessage", async (string message, IPublisher<string> publisher) => 
    {
        var result = await publisher.ForExchange("TOPIC_EXCHANGE").ForRouting("topic.2").Publish(message);
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
app.MapGet($"{context}/Fanout/SendMessage", async (string message, IPublisher<string> publisher) => 
    {
        publisher.ForExchange("FANOUT_EXCHANGE");
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

// http://localhost:5106/Fanout/SendMessage?message=Hello%20World
app.MapGet($"{context}/HealthReport", (IPublisher<string> publisher) => 
    {
        return publisher.ReportHealthStatus();
    })
    .WithName($"Health Check Report")
    .WithDisplayName("Health Check Report");

// http://localhost:5106/Fanout/SendMessage?message=Hello%20World
app.MapGet($"{context}/HealthReport/Summarized", async (IPublisher<string> publisher) => 
    {
        return (await publisher.ReportHealthStatus()).Summarize();
    })
    .WithName($"Sumarized Health Check Report")
    .WithDisplayName("Health Check Report");

app.MapPost($"{context}/Load/SendMessage", (
        LoadRequest request,
        IRabbitConnectionPool connectionPool,
        ILoggerFactory loggerFactory,
        IOptions<RabbitPublisherConfig> config
    ) =>
    {
        return Send(request, connectionPool, loggerFactory, config); 
    })
    .WithName("Configurable Load Test Runner")
    .WithDisplayName("Configurable Load Test Runner");

static IResult Send(LoadRequest request, 
        IRabbitConnectionPool connectionPool, 
        ILoggerFactory loggerFactory,
        IOptions<RabbitPublisherConfig> config) 
{
    ConcurrentDictionary<int, Stats> stats = new();
    List<Thread> threads = new();

    int idx = 0;

    IEnumerable<Message> msgs;
    if (request.IsBatch)
    {
        var msgArray = request.Message?.Split(";");
        msgs = msgArray?.Select(m => new Message { Text = m }) ?? new List<Message> { new() { Text = "Hello World" } };
    }
    else
    {
        msgs = new List<Message> { new() { Text = request.Message } };
    }

    for (int i = 0; i < request.Threads; ++i)
    {
        var t = new Thread(() =>
        {
            Stats stat = new();
            int k = Interlocked.Increment(ref idx);
            stats.TryAdd(k, stat);

            IPublisher<Message> publisher = new RabbitPublisher<Message>(connectionPool, loggerFactory, config);

            Stopwatch sw = new();
            for (int j = 0; j < request.NumberOfRequests; j++)
            {
                sw.Restart();

                string threadName = $"{Thread.CurrentThread.Name} ({Thread.CurrentThread.GetHashCode()}/{ThreadPool.ThreadCount})";

                publisher = request.ExchangeType switch
                {
                    "direct" => publisher.ForExchange("DIRECT_EXCHANGE").ForQueue("TEST_QUEUE"),
                    "topic" =>  publisher.ForExchange("TOPIC_EXCHANGE").ForRouting("topic.1"),
                    "fanout" => publisher.ForExchange("FANOUT_EXCHANGE"),
                    _ => null,
                } ?? throw new Exception($"Publisher not found for {request.ExchangeType}");

                Task<bool> task = request.IsBatch ? 
                    publisher.Publish(msgs) : publisher.Publish(msgs.First());

                try
                {
                    task.Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error publishing Message {msgs.First().Text} sent to {publisher.GetType().Name} - {ex.Message}");
                }
                sw.Stop();
                stat.TotalTime += sw.Elapsed;
                
                if (task.IsCompletedSuccessfully && task.Result)
                {
                    stat.Success += msgs.Count();
                }
                else
                {
                    stat.Errors++;
                }

                stat.TotalCount += msgs.Count();
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

app.MapGet($"{context}/Pool/Statistics", 
    (IRabbitConnectionPool connectionPool) => TypedResults.Ok(connectionPool.Stats))
    .WithName($"Pool Stats")
    .WithDisplayName("Stats");

app.Run();

/// <summary>
/// The configuration for the Load Test Runner
/// </summary> <summary>
/// 
/// </summary>
public class LoadRequest
{
    /// <summary>
    /// The Message to send
    /// </summary> <summary>
    /// 
    /// </summary>
    /// <value></value>
    public string? Message { get; set; }
    public string? ExchangeType { get; set; }
    public bool IsBatch { get; set; }
    public int Threads { get; set; }
    public int LagMillisecond { get; set; }
    public int NumberOfRequests { get; set; }

}

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

