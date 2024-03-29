using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Mime;
using FastCSharp.Publisher;
using FastCSharp.RabbitPublisher.Common;
using FastCSharp.RabbitPublisher.Impl;
using FastCSharp.RabbitPublisher.Injection;
using FastCSharp.RabbitPublisher.Test;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
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


var app = builder.Build();
app.UseRouting();
// http://localhost:5106/swagger/v1/swagger.json
app.UseSwagger();
// http://localhost:5106/swagger
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fast Framework.Test API");
});

ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

var defaultEndpoints = new Endpoints(loggerFactory, "rabbitsettings.json");
defaultEndpoints.Register(app, "", "Default VHost");

var vhostEndpoints = new Endpoints(loggerFactory, "rabbitsettings.VHOST.json");
vhostEndpoints.Register(app, "/test-vhost", "VHost test-vhost");

var batchDefaultEndpoints = new Endpoints(loggerFactory, "rabbitsettings.json", true);
batchDefaultEndpoints.Register(app, "/batch", "Batch Default VHost");

var batchVhostEndpoints = new Endpoints(loggerFactory, "rabbitsettings.VHOST.json", true);
batchVhostEndpoints.Register(app, "/batch-vhost", "Batch VHost");

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("rabbitsettings.VHOST.json", true, true)
    .Build();
var section = configuration.GetSection(RabbitOptions.SectionName);
RabbitOptions options = new();
section.Bind(options.Value);

var connectionPool = new RabbitConnectionPool(options.Value, loggerFactory);

app.MapPost("Load/Send", Load(connectionPool, loggerFactory, options.Value))
    .WithName("Configurable Test Runner")
    .WithDisplayName("Configurable Test Runner")
    .Produces(StatusCodes.Status200OK)
    .Accepts<LoadRequest>(MediaTypeNames.Application.Json)
    ;

app.Run();

static Func<LoadRequest, IResult> Load(RabbitConnectionPool pool, ILoggerFactory loggerFactory, RabbitPublisherConfig options)
{

    return IResult (
        LoadRequest request
    ) =>
    {


        var msgArray = request.Message?.Split(";");
        var msgs = msgArray?.Select(m => new Message { Text = m });
        msgs ??= new List<Message> { new Message { Text = "Hello World" } };
        
        ConcurrentDictionary<int, Stats> stats = new ConcurrentDictionary<int, Stats>();
        List<Thread> threads = new ();

        int idx = 0;
        
        for(int i = 0; i < request.Threads; ++i)
        {
            var t = new Thread(() =>
            {
                using IPublisher<Message> publisher = new RabbitPublisher<Message>(pool, loggerFactory, options);
                publisher.ForExchange("DIRECT_EXCHANGE").ForQueue("TEST_QUEUE");
                // return await Publish(message, publisher);

                switch (request.ExchangeType)
                {
                    case "direct":
                        publisher.ForExchange("DIRECT_EXCHANGE").ForQueue("TEST_QUEUE");
                        break;
                    case "topic":
                        publisher.ForExchange("TOPIC_EXCHANGE").ForRouting("topic.1");
                        break;
                    case "fanout":
                        publisher.ForExchange("FANOUT_EXCHANGE");
                        break;
                }

                Stats stat = new()
                {
                    TotalCount = 0,
                    MinTime = TimeSpan.MaxValue,
                    MaxTime = TimeSpan.MinValue,
                    TotalTime = TimeSpan.Zero,
                };
                int k = Interlocked.Increment(ref idx);
                stats.TryAdd(k, stat);

                Stopwatch sw = new();
                for (int j = 0; j < request.NumberOfRequests; j++)
                {
                    sw.Restart();
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
        foreach(var t in threads)
        {
            t.Join();
            Console.WriteLine($"Thread {t.Name} finished");
        }
        Console.WriteLine($"ThreadPool final size {ThreadPool.ThreadCount} >> Load Test Finished!");
        var totals = stats.Aggregate(
            new Stats(){
                Errors = 0,
                Success = 0,
                TotalCount = 0,
                TotalTime = TimeSpan.Zero,
                MinTime = TimeSpan.MaxValue,
                MaxTime = TimeSpan.MinValue
            }, 
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
    };
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
}
public class LoadRequest
{
    public string? Message { get; set; }
    public string? ExchangeType { get; set; }
    public bool IsBatch { get; set; }
    public int Threads { get; set; }
    public int LagMillisecond { get; set; }
    public int NumberOfRequests { get; set; }
}
