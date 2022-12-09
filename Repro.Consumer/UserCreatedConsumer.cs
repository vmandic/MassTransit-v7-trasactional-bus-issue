#nullable enable

using System;
using System.Threading.Tasks;
using GreenPipes;
using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.Definition;
using MassTransit.RabbitMqTransport;
using Repro.Dtos;

namespace Repro.Consumer
{
    public sealed class UserCreatedConsumer :
        IConsumer<IUserCreated1>,
        IConsumer<IUserCreated2>
    {
        public UserCreatedConsumer() { }

        public Task Consume(ConsumeContext<IUserCreated1> context)
        {
            Console.WriteLine($"{DateTime.Now} Consuming IUserCreated ->1 ...");
            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<IUserCreated2> context)
        {
            Console.WriteLine($"{DateTime.Now} Consuming IUserCreated ->2 ...");
            return Task.CompletedTask;
        }
    }

    public class UserCreatedConsumerDefinition : BaseConsumerDefinition<UserCreatedConsumer>
    {
        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<UserCreatedConsumer> consumerConfigurator)
        {
            base.ConfigureConsumer(endpointConfigurator, consumerConfigurator);
            endpointConfigurator.UseRateLimit(2, TimeSpan.FromSeconds(2));
        }
    }

    public abstract class BaseConsumerDefinition<TConsumer> :
        ConsumerDefinition<TConsumer>
        where TConsumer : class, IConsumer
    {
        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<TConsumer> consumerConfigurator)
        {
            endpointConfigurator.ConcurrentMessageLimit = 2;
            endpointConfigurator.PrefetchCount = 16;

            // Retry in-memory 2 times right away.
            // Note: these retries block further messages from being processed by the consumer!
            endpointConfigurator.UseMessageRetry(r => r.Immediate(2));

            // Retry after 5, 15, 30, 60 and 120 minutes.
            endpointConfigurator.UseDelayedRedelivery(r =>
                r.Intervals(
                    TimeSpan.FromMinutes(5),
                    TimeSpan.FromMinutes(15),
                    TimeSpan.FromMinutes(30),
                    TimeSpan.FromMinutes(60),
                    TimeSpan.FromMinutes(120)
                )
            );

            if (endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rabbitMqConfigurator)
            {
                rabbitMqConfigurator.SetQueueArgument("x-queue-type", "quorum");
            }
        }
    }
}
