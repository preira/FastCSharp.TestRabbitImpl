using FastCSharp.Publisher;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRabbitPublisher<string>(builder.Configuration);

var app = builder.Build();


app.MapGet("/", async (string message, IRabbitPublisher<string> publisher) => {
    return await publisher.ForExchange("DIRECT_EXCHANGE").Publish(message);
});

app.Run();
