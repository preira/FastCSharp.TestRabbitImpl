using FastCSharp.Publisher;
using FastCSharp.RabbitPublisher.Common;
using FastCSharp.RabbitPublisher.Impl;
using FastCSharp.RabbitPublisher.Injection;

var builder = WebApplication.CreateBuilder(args);

RabbitOptions options = new();
builder.Configuration.GetSection(RabbitOptions.SectionName).Bind(options.Value);

ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

var connectionPool = new RabbitConnectionPool(options.Value, loggerFactory);


var app = builder.Build();

app.MapGet("/", async (string message) => {
    IRabbitPublisher<string> publisher = new RabbitPublisher<string>(connectionPool, loggerFactory, options);
    return await publisher.ForExchange("DIRECT_EXCHANGE").Publish(message);
});

app.Run();
