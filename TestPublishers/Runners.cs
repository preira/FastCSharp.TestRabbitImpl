using FastCSharp.Publisher;

namespace FastCSharp.RabbitPublisher.Test;
public interface ITestPublisher<T> : IPublisher<T>, IBatchPublisher<T>
{
}

public interface IRunner<T>
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
    IPublisher<T> publisher;

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
    IBatchPublisher<T> publisher;

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
    public ITestPublisher<T> TopicPublisher1 { get; set; }
    public ITestPublisher<T> TopicPublisher2 { get; set; }
    public ITestPublisher<T> FanoutPublisher { get; set; }
    public ITestPublisher<T> DirectPublisher { get; set; }
    public Runner(string config)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile(config, true, true)
            .Build();
        ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        IPublisherFactory topicFactory = new RabbitTopicPublisherFactory(configuration, loggerFactory);
        TopicPublisher1 = new SingleMsgTestPublisher<T>(topicFactory.NewPublisher<T>("TOPIC_EXCHANGE", "topic.1"));
        TopicPublisher2 = new SingleMsgTestPublisher<T>(topicFactory.NewPublisher<T>("TOPIC_EXCHANGE", "topic.2"));

        IPublisherFactory fanoutFactory = new RabbitFanoutPublisherFactory(configuration, loggerFactory);
        FanoutPublisher = new SingleMsgTestPublisher<T>(fanoutFactory.NewPublisher<T>("FANOUT_EXCHANGE"));

        IPublisherFactory directFactory = new RabbitDirectPublisherFactory(configuration, loggerFactory);
        DirectPublisher = new SingleMsgTestPublisher<T>(directFactory.NewPublisher<T>("DIRECT_EXCHANGE", "TEST_QUEUE"));

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

    public Task Run(ITestPublisher<T> publisher, IEnumerable<T> message)
    {
        throw new NotImplementedException();
    }
}
public class BatchRunner<T> : IRunner<T>
{
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
        IBatchPublisherFactory topicFactory = new RabbitTopicBatchPublisherFactory(configuration, loggerFactory);
        TopicPublisher1 = new BatchTestPublisher<T>(topicFactory.NewPublisher<T>("TOPIC_EXCHANGE", "topic.1"));
        TopicPublisher2 = new BatchTestPublisher<T>(topicFactory.NewPublisher<T>("TOPIC_EXCHANGE", "topic.2"));

        IBatchPublisherFactory fanoutFactory = new RabbitFanoutBatchPublisherFactory(configuration, loggerFactory);
        FanoutPublisher = new BatchTestPublisher<T>(fanoutFactory.NewPublisher<T>("FANOUT_EXCHANGE"));

        IBatchPublisherFactory directFactory = new RabbitDirectBatchPublisherFactory(configuration, loggerFactory);
        DirectPublisher = new BatchTestPublisher<T>(directFactory.NewPublisher<T>("DIRECT_EXCHANGE", "TEST_QUEUE"));

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
}

