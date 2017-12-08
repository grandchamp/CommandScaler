using CommandScaler.Bus.Contracts;
using CommandScaler.RabbitMQ.Connection;
using CommandScaler.RabbitMQ.Connection.Contracts;
using CommandScaler.RabbitMQ.Handler;
using CommandScaler.RabbitMQ.Manager;
using Microsoft.Extensions.DependencyInjection;

namespace CommandScaler
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitMQCommandScaler(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IRabbitConnectionManager, RabbitConnectionManager>();
            serviceCollection.AddScoped<IBus, RabbitBus>();

            return serviceCollection;
        }

        public static void ConfigureRabbitMQHandler(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<RabbitGenericHandler>();
        }
    }
}
