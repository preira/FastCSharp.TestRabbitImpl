# Build RabbitMQ solution
Run the setup-amq.bat in windows to create the RabbitMQ solution.  
Alternatively, you can create and run a setup-amq.sh in linux to create the RabbitMQ solution.  
You can also create the solution manually through the RabbitMQ management console.  

Use the teardown-amq.bat to remove the RabbitMQ solution.  

The solution architecture is shown below.

## Architecture
```mermaid	
graph LR
subgraph RabbitMQ
    %% DIRECT /
    Direct{"`**Exchange**
        test.direct.exchange.v-1.0`"}
    QDirect[("`**Queue**
                test.direct.q`")]
    Direct --> QDirect

    %% TOPIC /
    Topic{"`**Exchange**
        test.topic.exchange.v-1.0`"}
    QTopic1[("`**Queue**
                test.topic.q.1`")]
    QTopic2[("`**Queue**
                test.topic.q.2`")]
    Topic -- routing key: `topic.1` --> QTopic1
    Topic -- routing key: `topic.2` --> QTopic2

    %% FANOUT /
    Fanout{"`**Exchange**
        test.fanout.exchange.v-1.0`"}
    QFanout1[("`**Queue**
                test.fanout.q.1`")]
    QFanout2[("`**Queue**
                test.fanout.q.2`")]
    Fanout --> QFanout1
    Fanout --> QFanout2

    subgraph vhost-test
        %% DIRECT vhost-test/
        vDirect{"`**Exchange**
            test.direct.exchange.v-1.0`"}
        vQDirect[("`**Queue**
                    test.direct.q`")]
        vDirect --> vQDirect

        %% TOPIC /
        vTopic{"`**Exchange**
            test.topic.exchange.v-1.0`"}
        vQTopic1[("`**Queue**
                    test.topic.q.1`")]
        vQTopic2[("`**Queue**
                    test.topic.q.2`")]
        vTopic -- routing key: `topic.1` --> vQTopic1
        vTopic -- routing key: `topic.2` --> vQTopic2

        %% FANOUT /
        vFanout{"`**Exchange**
            test.fanout.exchange.v-1.0`"}
        vQFanout1[("`**Queue**
                    test.fanout.q.1`")]
        vQFanout2[("`**Queue**
                    test.fanout.q.2`")]
        vFanout --> vQFanout1
        vFanout --> vQFanout2

    end
end 

WebAPI["`**TestPublisher WebAPI**
    *+SendDirectMessage*
    *+SendTopicMessage*
    *+SendFanoutMessage*
    *+SendDirectMessageVHost*
    *+SendTopicMessageVHost*
    *+SendFanoutMessageVHost*
    *+LoadTest*`"]

%% Producer and Consumer
DirectProducer["`TestPublisher Producer 
    < **SendDirectMessage** >`"] 
TopicProducer["`TestPublisher Producer 
    < **SendTopicMessage** >`"] 
FanoutProducer["`TestPublisher Producer 
    < **SendFanoutMessage** >`"] 
Consumer["`MQConsumer 
        < **MQConsumer** >`"]

WebAPI --> DirectProducer
WebAPI --> TopicProducer
WebAPI --> FanoutProducer
WebAPI --> DirectProducer
WebAPI --> TopicProducer
WebAPI --> FanoutProducer

DirectProducer --> Direct
TopicProducer --> Topic
FanoutProducer --> Fanout
DirectProducer --> vDirect
TopicProducer --> vTopic
FanoutProducer --> vFanout

QDirect --> Consumer
QTopic1 --> Consumer
QTopic2 --> Consumer
QFanout1 --> Consumer
QFanout2 --> Consumer
vQDirect --> Consumer
vQTopic1 --> Consumer
vQTopic2 --> Consumer
vQFanout1 --> Consumer
vQFanout2 --> Consumer


```
