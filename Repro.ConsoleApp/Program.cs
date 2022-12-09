using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MassTransit;
using Repro.Dtos;
using System.Reflection;
using MassTransit.Transactions;

namespace Repro.ConsoleApp
{
    public static class MassTransitExtensions
    {
        public static void AddMassTransitUsingRabbitMq(
            this IServiceCollection serviceCollection)
        {
            var hosts = new {
                Username = "guest",
                Password = "guest",
                HostAddress = "localhost:5672",
                VirtualHost = "/"
            };

            serviceCollection.AddMassTransit(bus =>
            {
                bus.UsingRabbitMq((context, rabbitMqconfig) =>
                {
                    var host = GetRabbitMqHostUri(hosts.Username, hosts.Password, hosts.HostAddress, hosts.VirtualHost);
                    rabbitMqconfig.Host(host);
                   
                    rabbitMqconfig.ConfigureEndpoints(context);

                    Console.WriteLine("MassTransit endpoints configured: " + host);
                });

                // THIS IS THE ISSUE: if we comment this out it works...
                bus.AddTransactionalEnlistmentBus();
            });

            // Starts a background service that interacts with the MQ
            serviceCollection.AddMassTransitHostedService(true);

            Console.WriteLine("MassTransit + RabbitMq configured");
        }

        private static string GetRabbitMqHostUri(string? username, string? password, string hostAddress, string? virtualHost)
        {
            return $"amqp://{username}:{password}@{hostAddress}{virtualHost}";
        }
    }

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using var scope = host.Services.CreateScope();
            var publisher = scope.ServiceProvider.GetService<IPublishEndpoint>();
            
            Console.WriteLine("Dispatching 1...");
            // NOTE: never shown in Admin UI, never comes to RabbitMQ
            await publisher.Publish<IUserCreated1>(new { UserId = 123 });
            Console.WriteLine("Message 1 dispatched.");
            
            Console.WriteLine("Dispatching 2...");
            await publisher.Publish<IUserCreated2>(new { UserId = 321 }); 
            Console.WriteLine("Message 2 dispatched.");
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            if (string.IsNullOrEmpty(environment))
            {
                environment = Environments.Development;
            }

            var host = Host.CreateDefaultBuilder(args)
                .UseEnvironment(environment)
                .UseDefaultServiceProvider((_, options) =>
                {
                    options.ValidateScopes = true;
                    options.ValidateOnBuild = true;
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddMassTransitUsingRabbitMq();
                })
                .ConfigureLogging((c, l) =>
                {
                    l.AddConfiguration(c.Configuration);
                });

            return host;
        }
    }
}
