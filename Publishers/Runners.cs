using FastCSharp.Publisher;

namespace FastCSharp.RabbitPublisher.Test;
public interface ITestPublisher<T> : IPublisher<T>, IBatchPublisher<T>
{
}

public interface IRunner<T> : IDisposable
{
    ITestPublisher<T> TopicPublisher1 { get; set; }
    ITestPublisher<T> TopicPublisher2 { get; set; }
    ITestPublisher<T> FanoutPublisher { get; set; }
    ITestPublisher<T> DirectPublisher { get; set; }
    Task Run(ITestPublisher<T> publisher, T message);
    Task Run(ITestPublisher<T> publisher, IEnumerable<T> message);
}

public class SingleMsgTestPublisher<T> : ITestPublisher<T>
{
    readonly IPublisher<T> publisher;

    public SingleMsgTestPublisher(IPublisher<T> publisher)
    {
        this.publisher = publisher;
    }

    public Task<bool> Publish(T? message)
    {
        return publisher.Publish(message);
    }

    public Task<bool> BatchPublish(IEnumerable<T> message)
    {
        throw new NotImplementedException();
    }

    public IHandler<T> AddMsgHandler(Handler<T> handler)
    {
        return publisher.AddMsgHandler(handler);
    }

    public void Dispose()
    {
        publisher.Dispose();
    }
}

public class BatchTestPublisher<T> : ITestPublisher<T>
{
    readonly IBatchPublisher<T> publisher;

    public BatchTestPublisher(IBatchPublisher<T> publisher)
    {
        this.publisher = publisher;
    }

    public Task<bool> Publish(T? message)
    {
        throw new NotImplementedException();
    }

    public Task<bool> BatchPublish(IEnumerable<T> message)
    {
        return publisher.BatchPublish(message);
    }

    public IHandler<T> AddMsgHandler(Handler<T> handler)
    {
        return publisher.AddMsgHandler(handler);
    }

    public void Dispose()
    {
        publisher.Dispose();
    }
}

public class Runner<T> : IRunner<T>
{
    private bool disposedValue;

    IPublisherFactory TopicFactory { get; set; }
    IPublisherFactory FanoutFactory { get; set; }
    IPublisherFactory DirectFactory { get; set; }
    public ITestPublisher<T> TopicPublisher1 { get; set; }
    public ITestPublisher<T> TopicPublisher2 { get; set; }
    public ITestPublisher<T> FanoutPublisher { get; set; }
    public ITestPublisher<T> DirectPublisher { get; set; }
    public Runner(string config)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile(config, true, true)
            .Build();
        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        TopicFactory = new RabbitTopicPublisherFactory(configuration, loggerFactory);
        TopicPublisher1 = new SingleMsgTestPublisher<T>(TopicFactory.NewPublisher<T>("TOPIC_EXCHANGE", "topic.1"));
        TopicPublisher2 = new SingleMsgTestPublisher<T>(TopicFactory.NewPublisher<T>("TOPIC_EXCHANGE", "topic.2"));

        FanoutFactory = new RabbitFanoutPublisherFactory(configuration, loggerFactory);
        FanoutPublisher = new SingleMsgTestPublisher<T>(FanoutFactory.NewPublisher<T>("FANOUT_EXCHANGE"));

        DirectFactory = new RabbitDirectPublisherFactory(configuration, loggerFactory);
        DirectPublisher = new SingleMsgTestPublisher<T>(DirectFactory.NewPublisher<T>("DIRECT_EXCHANGE", "TEST_QUEUE"));

        Console.WriteLine($"ThreadPool initial size {ThreadPool.ThreadCount} >> Runner Initialized!");
    }

    public async Task Run(ITestPublisher<T> publisher, T message)
    {
        string threadName = $"{Thread.CurrentThread.Name} ({Thread.CurrentThread.GetHashCode()}/{ThreadPool.ThreadCount})";
        bool isSent = await publisher.Publish(message);
        if (!isSent)
        {
            throw new Exception($"publisher 2 {threadName} >> Message not sent!");
        }
        Console.WriteLine($"publisher 2 {threadName} >> Message Sent!");
    }

    public async Task Run(ITestPublisher<T> publisher, IEnumerable<T> message)
    {
        await Run(publisher, message.First());
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // dispose managed state (managed objects)
                TopicPublisher1.Dispose();
                TopicPublisher2.Dispose();
                FanoutPublisher.Dispose();
                DirectPublisher.Dispose();

                TopicFactory.Dispose();
                FanoutFactory.Dispose();
                DirectFactory.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
public class BatchRunner<T> : IRunner<T>
{
    private bool disposedValue;

    IBatchPublisherFactory TopicFactory { get; set; }
    IBatchPublisherFactory FanoutFactory { get; set; }
    IBatchPublisherFactory DirectFactory { get; set; }
    public ITestPublisher<T> TopicPublisher1 { get; set; }
    public ITestPublisher<T> TopicPublisher2 { get; set; }
    public ITestPublisher<T> FanoutPublisher { get; set; }
    public ITestPublisher<T> DirectPublisher { get; set; }
    public BatchRunner(string config)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile(config, true, true)
            .Build();
        ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        TopicFactory = new RabbitTopicBatchPublisherFactory(configuration, loggerFactory);
        TopicPublisher1 = new BatchTestPublisher<T>(TopicFactory.NewPublisher<T>("TOPIC_EXCHANGE", "topic.1"));
        TopicPublisher2 = new BatchTestPublisher<T>(TopicFactory.NewPublisher<T>("TOPIC_EXCHANGE", "topic.2"));

        FanoutFactory = new RabbitFanoutBatchPublisherFactory(configuration, loggerFactory);
        FanoutPublisher = new BatchTestPublisher<T>(FanoutFactory.NewPublisher<T>("FANOUT_EXCHANGE"));

        DirectFactory = new RabbitDirectBatchPublisherFactory(configuration, loggerFactory);
        DirectPublisher = new BatchTestPublisher<T>(DirectFactory.NewPublisher<T>("DIRECT_EXCHANGE", "TEST_QUEUE"));

        Console.WriteLine($"ThreadPool initial size {ThreadPool.ThreadCount} >> Runner Initialized!");
    }

    public async Task Run(ITestPublisher<T> publisher, IEnumerable<T> messages)
    {
        string threadName = $"{Thread.CurrentThread.Name} ({Thread.CurrentThread.GetHashCode()}/{ThreadPool.ThreadCount})";

        bool isSent = await publisher.BatchPublish(messages);
        if (!isSent)
        {
            throw new Exception($"publisher 2 {threadName} >> Message not sent!");
        }
        Console.WriteLine($"publisher 2 {threadName} >> Message Sent!");
    }

    public Task Run(ITestPublisher<T> publisher, T message)
    {
        throw new NotImplementedException();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // dispose managed state (managed objects)
                TopicPublisher1.Dispose();
                TopicPublisher2.Dispose();
                FanoutPublisher.Dispose();
                DirectPublisher.Dispose();

                TopicFactory.Dispose();
                FanoutFactory.Dispose();
                DirectFactory.Dispose();
            }
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

