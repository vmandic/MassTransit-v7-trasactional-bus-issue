using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MassTransit;
using System.Reflection;
using MassTransit.Transactions;

namespace Repro.Consumer
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
                bus.AddConsumers(Assembly.GetEntryAssembly());

                bus.UsingRabbitMq((context, rabbitMqconfig) =>
                {
                    string host = GetRabbitMqHostUri(hosts.Username, hosts.Password, hosts.HostAddress, hosts.VirtualHost);
                    rabbitMqconfig.Host(host);
                   
                    rabbitMqconfig.ConfigureEndpoints(context);
                });

                // THIS IS THE ISSUE: if we comment this out it works...
                bus.AddTransactionalEnlistmentBus();
            });

            // Starts a background service that interacts with the MQ
            serviceCollection.AddMassTransitHostedService(true);
        }

        private static string GetRabbitMqHostUri(string username, string password, string hostAddress, string virtualHost)
        {
            return $"amqp://{username}:{password}@{hostAddress}/{virtualHost}";
        }
    }

    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
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
                    // NOTE: keeps it running
                    services.AddHostedService<Worker>();

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
