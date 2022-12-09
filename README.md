# MassTransit-v7-trasactional-bus-issue
Demonstration of v7 MassTransit + RabbitMq bus.AddTransactionalEnlistmentBus() not working in v7 and working in v8

To test open first commit for v7 and then open v8 for fix.

Start Repro.Consumer and then launch few times Repro.ConsoleApp to dispatch message.

bus.AddTransactionalEnlistmentBus() causes the Repro.ConsoleApp not to connect to RabbitMq broker which you can verify in the logs. The messages are not dispatched, also visible from no logger output in Repro.ConsoleApp.

### Author
Vedran Mandić
