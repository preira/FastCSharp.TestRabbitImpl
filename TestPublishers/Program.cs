using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Mime;
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


var defaultRunner = new Runner<Message>("rabbitsettings.json");
var defaultEndpoints = new Endpoints(defaultRunner);
defaultEndpoints.Register(app, "", "Default VHost");

var vhostRunner = new Runner<Message>("rabbitsettings.VHOST.json");
var vhostEndpoints = new Endpoints(vhostRunner);
vhostEndpoints.Register(app, "/test-vhost", "VHost test-vhost");

var batchDefaultRunner = new BatchRunner<Message>("rabbitsettings.json");
var batchDefaultEndpoints = new Endpoints(batchDefaultRunner);
batchDefaultEndpoints.Register(app, "/batch", "Batch Default VHost");

var batchVhostRunner = new BatchRunner<Message>("rabbitsettings.VHOST.json");
var batchVhostEndpoints = new Endpoints(batchVhostRunner);
batchVhostEndpoints.Register(app, "/batch-vhost", "Batch VHost");

app.MapPost("Load/SendMessage", Load())
    .WithName("Load Test")
    .WithDisplayName("Load Test")
    .Produces(StatusCodes.Status200OK)
    .Accepts<LoadRequest>(MediaTypeNames.Application.Json)
    ;

app.Run();

static Func<LoadRequest, IResult> Load()
{
    return IResult (
        LoadRequest request
    ) =>
    {
        if(request.VHost == null || request.VHost == "" || request.VHost == "/")
        {
            request.VHost = "rabbitsettings.json";
        }
        else
        {
            request.VHost = "rabbitsettings.VHOST.json";
        }

        IRunner<Message> runner;
        if(request.IsBatch)
        {
            runner = new BatchRunner<Message>(request.VHost);
        }
        else
        {
            runner = new Runner<Message>(request.VHost);
        }

        ITestPublisher<Message> publisher;
        switch (request.ExchangeType)
        {
            case "direct":
                publisher = runner.DirectPublisher;
                break;
            case "topic":
                publisher = runner.TopicPublisher1;
                break;
            case "fanout":
                publisher = runner.FanoutPublisher;
                break;
            default:
                return TypedResults.BadRequest(request.ExchangeType);
        }

        var msgArray = request.Message?.Split(";");
        var msgs = new List<Message>();
        if (msgArray?.Length > 0)
        {
            foreach (var msg in msgArray)
            {
                var m = new Message
                {
                    Text = msg
                };
                msgs.Add(m);
            }
        }
        else
        {
            var m = new Message
            {
                Text = "Hello World"
            };
            msgs.Add(m);
        }
        ConcurrentDictionary<int, Stats> stats = new ConcurrentDictionary<int, Stats>();
        List<Thread> threads = new ();

        int idx = 0;
        
        for(int i = 0; i < request.Threads; ++i)
        {
            var t = new Thread(() =>
            {
                Stats stat = new()
                {
                    TotalCount = 0,
                    MinTime = TimeSpan.MaxValue,
                    MaxTime = TimeSpan.MinValue,
                    TotalTime = TimeSpan.Zero,
                };
                int k = Interlocked.Increment(ref idx);
                stats.TryAdd(k, stat);

                for (int j = 0; j < request.NumberOfRequests; j++)
                {
                    Stopwatch sw = new();
                    sw.Start();
                    try
                    {
                        Task task;
                        if (request.IsBatch)
                        {
                            task = runner.Run(publisher, msgs);
                        }
                        else
                        {
                            task = runner.Run(publisher, msgs.First());
                        }
                        task.Wait();
                        stat.Success++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        stat.Errors++;
                    }
                    sw.Stop();
                    stat.TotalCount += msgs.Count;
                    stat.TotalTime += sw.Elapsed;
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
    public string? VHost { get; set; }
    public string? Message { get; set; }
    public string? ExchangeType { get; set; }
    public bool IsBatch { get; set; }
    public int Threads { get; set; }
    public int LagMillisecond { get; set; }
    public int NumberOfRequests { get; set; }
}
