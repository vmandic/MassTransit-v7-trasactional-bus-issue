# MassTransit-v7-trasactional-bus-issue
Demonstration of v7 MassTransit + RabbitMq bus.AddTransactionalEnlistmentBus() not working in v7 and working in v8

To test you need to start rabbitmq locally with a docker compose first, open [first commit](https://github.com/vmandic/MassTransit-v7-trasactional-bus-issue/commit/e1ced83eb98125355c75162574b58fb8f24a17b8) for v7 and then open [v8 commit](https://github.com/vmandic/MassTransit-v7-trasactional-bus-issue/commit/862fcfb1f5e72c4886357d1c4309b7ab5b58c82c) for fix.

Start Repro.Consumer and then launch few times Repro.ConsoleApp to dispatch message.

bus.AddTransactionalEnlistmentBus() causes the Repro.ConsoleApp not to connect to RabbitMq broker which you can verify in the logs. The messages are not dispatched, also visible from no logger output in Repro.ConsoleApp.

### Author
Vedran Mandić
