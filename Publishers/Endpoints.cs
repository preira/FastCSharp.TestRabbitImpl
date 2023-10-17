namespace FastCSharp.RabbitPublisher.Test;
public class Message
{
    public Message()
    {
    }

    public string? Text { get; set; }
}
public class Endpoints
{
    IRunner<Message> runner;
    public Endpoints(IRunner<Message> runner)
    {
        this.runner = runner;
    }

    public WebApplication Register(WebApplication app, string context, string displayName)
    {
        app.MapGet($"{context}/Direct/SendMessage", Publish(runner, runner.DirectPublisher))
            .WithName($"Direct for {context}")
            .WithDisplayName(displayName);

        // http://localhost:5106/Topic/topic.1/SendMessage?message=Hello%20World
        app.MapGet($"{context}/Topic/topic.1/SendMessage", Publish(runner, runner.TopicPublisher1))
            .WithName($"Topic for {context} topic.1")
            .WithDisplayName(displayName);

        // http://localhost:5106/Topic/topic.2/SendMessage?message=Hello%20World
        app.MapGet($"{context}/Topic/topic.2/SendMessage", Publish(runner, runner.TopicPublisher2))
            .WithName($"Topic for {context} topic.2")
            .WithDisplayName(displayName);

        // http://localhost:5106/Fanout/SendMessage?message=Hello%20World
        app.MapGet($"{context}/Fanout/SendMessage", Publish(runner, runner.FanoutPublisher))
            .WithName($"Fanout for {context}")
            .WithDisplayName(displayName);

        return app;
    }

    static Func<string?, Task<IResult>> Publish(IRunner<Message> runner, ITestPublisher<Message> publisher)
    {
        if (runner.IsBatch)
        {
            return async Task<IResult> (string? message) =>
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
                    await runner.Run(publisher, msgs);
                    return TypedResults.Accepted("");
                }
                return TypedResults.BadRequest(message);
            };
        }
        else
        {
            return async Task<IResult> (string? message) =>
            {
                if (message == null)
                {
                    return TypedResults.BadRequest(message);
                }
                var m = new Message
                {
                    Text = message
                };
                await runner.Run(publisher, m);
                return TypedResults.Accepted("");
            };
        }
    }
}
