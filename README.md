# FastCSharp.TestRabbitImpl
Simple working usage examples for FastCSharp's RabbitPublisher and RabbitSubscriber 
## How to use
1. Clone this repository
2. Open the solution in Visual Studio Code
3. Run a RabbitMQ server (e.g. using Docker) in the default port (5672).
4. Run setup-amq.bat to create the required exchanges and queues. Or follow the instructions in this [page](./ConfigureRabbit/README.md).
5. Run the publisher and subscriber projects in different consoles.
6. Use the Swagger UI to send messages to the publisher. Or use the browser with the url http://localhost:5106/SendDirectMessage?message=Hello (you can change the message parameter to whatever you want).  
8. Check the console of the subscriber to see the messages received.  
9. Check the console of the publisher to see the messages sent confirmation.  

You can stop the subscriber and check the RabbitMQ management page to see the messages in the queues. Starting the subscriber again will consume the messages in the queues.  