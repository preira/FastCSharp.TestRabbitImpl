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
IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("rabbitsettings.json", true, true)
    .Build();

services.AddRabbitPublisher(configuration);

var app = builder.Build();
app.UseRouting();
// http://localhost:5106/swagger/v1/swagger.json
app.UseSwagger();
// http://localhost:5106/swagger
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fast Framework.Test API");
});
var context = "single";
var displayName = "Default VHost";
app.MapGet($"{context}/Direct/SendMessage", async (string message, IPublisherFactory<IDirectPublisher> publisherFactory) => {
    var publisher = publisherFactory.NewPublisher<string>("DIRECT_EXCHANGE", "TEST_QUEUE");
    var result = await publisher.Publish(message);
    if(result)
    {
        Console.WriteLine($"Message {message} sent to {publisher.GetType().Name}");
        return Results.Ok("Success");
    }
    else 
    {
        Console.WriteLine($"Error publishing Message {message} sent to {publisher.GetType().Name}");
        return Results.Problem("Error");
    }
})
    .WithName($"Direct for {context}")
    .WithDisplayName(displayName);

// http://localhost:5106/Topic/topic.1/SendMessage?message=Hello%20World
app.MapGet($"{context}/Topic/topic.1/SendMessage", async (string message, IPublisherFactory<ITopicPublisher> publisherFactory) => {
    var publisher = publisherFactory.NewPublisher<string>("TOPIC_EXCHANGE", "topic.1");
    var result = await publisher.Publish(message);
    if(result)
    {
        Console.WriteLine($"Message {message} sent to {publisher.GetType().Name}");
        return Results.Ok("Success");
    }
    else 
    {
        Console.WriteLine($"Error publishing Message {message} sent to {publisher.GetType().Name}");
        return Results.Problem("Error");
    }
})
    .WithName($"Topic for {context} topic.1")
    .WithDisplayName(displayName);

// http://localhost:5106/Topic/topic.2/SendMessage?message=Hello%20World
app.MapGet($"{context}/Topic/topic.2/SendMessage", async (string message, IPublisherFactory<ITopicPublisher> publisherFactory) => {
    var publisher = publisherFactory.NewPublisher<string>("TOPIC_EXCHANGE", "topic.2");
    var result = await publisher.Publish(message);
    if(result)
    {
        Console.WriteLine($"Message {message} sent to {publisher.GetType().Name}");
        return Results.Ok("Success");
    }
    else 
    {
        Console.WriteLine($"Error publishing Message {message} sent to {publisher.GetType().Name}");
        return Results.Problem("Error");
    }
})
    .WithName($"Topic for {context} topic.2")
    .WithDisplayName(displayName);

// http://localhost:5106/Fanout/SendMessage?message=Hello%20World
app.MapGet($"{context}/Fanout/SendMessage", async (string message, IPublisherFactory<IFanoutPublisher> publisherFactory) => {
    var publisher = publisherFactory.NewPublisher<string>("FANOUT_EXCHANGE");
    var result = await publisher.Publish(message);
    if(result)
    {
        Console.WriteLine($"Message {message} sent to {publisher.GetType().Name}");
        return Results.Ok("Success");
    }
    else 
    {
        Console.WriteLine($"Error publishing Message {message} sent to {publisher.GetType().Name}");
        return Results.Problem("Error");
    }
})
    .WithName($"Fanout for {context}")
    .WithDisplayName(displayName);

app.Run();
