# Build RabbitMQ solution
Run the setup-amq.bat in windows to create the RabbitMQ solution.  
Alternatively, you can create and run a setup-amq.sh in linux to create the RabbitMQ solution.  
You can also create the solution manually through the RabbitMQ management console. 

## Architecture
```mermaid	
graph LR
A["`Producer <br>< **SendDirectMessage** >`"] --> B{"`**Exchange**<br>test.direct.exchange.v-1.0`"}
subgraph RabbitMQ
B --> C[("`**Queue**<br>test.direct.q`")]
end 
C --> D["`Consumer <br>< **MQConsumer** >`"]
```
