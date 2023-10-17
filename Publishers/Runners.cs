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
    bool IsBatch { get; }
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

    public IHandler<T> AddMsgHandler(Handler<T> handler)
    {
        return publisher.AddMsgHandler(handler);
    }

    public void Dispose()
    {
        publisher.Dispose();
    }

    public Task<bool> Publish(IEnumerable<T> messages)
    {
        throw new NotImplementedException();
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

    public Task<bool> Publish(IEnumerable<T> message)
    {
        return publisher.Publish(message);
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

public class Factories
{
    public IPublisherFactory<ITopicPublisher> TopicFactory { get; private set; }
    public IPublisherFactory<IFanoutPublisher> FanoutFactory { get; private set; }
    public IPublisherFactory<IDirectPublisher> DirectFactory { get; private set; }

    private Factories(string config)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile(config, true, true)
            .Build();
        ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        
        TopicFactory = new RabbitTopicPublisherFactory(configuration, loggerFactory);
        FanoutFactory = new RabbitFanoutPublisherFactory(configuration, loggerFactory);
        DirectFactory = new RabbitDirectPublisherFactory(configuration, loggerFactory);
    }

    private static Factories instance = null!;
    public static Factories Instance(string config)
    {
        if (instance == null)
        {
            instance = new Factories(config);
        }
        return instance;
    }
}

public class BatchFactories
{
    public IBatchPublisherFactory<ITopicPublisher> TopicFactory { get; private set; }
    public IBatchPublisherFactory<IFanoutPublisher> FanoutFactory { get; private set; }
    public IBatchPublisherFactory<IDirectPublisher> DirectFactory { get; private set; }

    private BatchFactories(string config)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile(config, true, true)
            .Build();
        ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        
        TopicFactory = new RabbitTopicBatchPublisherFactory(configuration, loggerFactory);
        FanoutFactory = new RabbitFanoutBatchPublisherFactory(configuration, loggerFactory);
        DirectFactory = new RabbitDirectBatchPublisherFactory(configuration, loggerFactory);
    }

    private static BatchFactories instance = null!;
    public static BatchFactories Instance(string config)
    {
        if (instance == null)
        {
            instance = new BatchFactories(config);
        }
        return instance;
    }
}

public class Runner<T> : IRunner<T>
{
    private bool disposedValue;

    readonly Factories factories;
    public ITestPublisher<T> TopicPublisher1 { get; set; }
    public ITestPublisher<T> TopicPublisher2 { get; set; }
    public ITestPublisher<T> FanoutPublisher { get; set; }
    public ITestPublisher<T> DirectPublisher { get; set; }
    public bool IsBatch { get => false; }

    public Runner(string config)
    {
        factories = Factories.Instance(config);

        TopicPublisher1 = new SingleMsgTestPublisher<T>(factories.TopicFactory.NewPublisher<T>("TOPIC_EXCHANGE", "topic.1"));
        TopicPublisher2 = new SingleMsgTestPublisher<T>(factories.TopicFactory.NewPublisher<T>("TOPIC_EXCHANGE", "topic.2"));

        FanoutPublisher = new SingleMsgTestPublisher<T>(factories.FanoutFactory.NewPublisher<T>("FANOUT_EXCHANGE"));

        DirectPublisher = new SingleMsgTestPublisher<T>(factories.DirectFactory.NewPublisher<T>("DIRECT_EXCHANGE", "TEST_QUEUE"));

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

    public bool IsBatch { get => true; }
    readonly BatchFactories factories;
    public ITestPublisher<T> TopicPublisher1 { get; set; }
    public ITestPublisher<T> TopicPublisher2 { get; set; }
    public ITestPublisher<T> FanoutPublisher { get; set; }
    public ITestPublisher<T> DirectPublisher { get; set; }
    public BatchRunner(string config)
    {
        factories = BatchFactories.Instance(config);

        TopicPublisher1 = new BatchTestPublisher<T>(factories.TopicFactory.NewPublisher<T>("TOPIC_EXCHANGE", "topic.1"));
        TopicPublisher2 = new BatchTestPublisher<T>(factories.TopicFactory.NewPublisher<T>("TOPIC_EXCHANGE", "topic.2"));
        FanoutPublisher = new BatchTestPublisher<T>(factories.FanoutFactory.NewPublisher<T>("FANOUT_EXCHANGE"));
        DirectPublisher = new BatchTestPublisher<T>(factories.DirectFactory.NewPublisher<T>("DIRECT_EXCHANGE", "TEST_QUEUE"));

        Console.WriteLine($"ThreadPool initial size {ThreadPool.ThreadCount} >> Runner Initialized!");
    }

    public async Task Run(ITestPublisher<T> publisher, IEnumerable<T> messages)
    {
        string threadName = $"{Thread.CurrentThread.Name} ({Thread.CurrentThread.GetHashCode()}/{ThreadPool.ThreadCount})";

        bool isSent = await publisher.Publish(messages);
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

