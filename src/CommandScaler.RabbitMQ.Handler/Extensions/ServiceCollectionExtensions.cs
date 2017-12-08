using CommandScaler.RabbitMQ.Handler.Handler;
using Microsoft.Extensions.DependencyInjection;

namespace CommandScaler
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureRabbitMQHandler(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<RabbitGenericHandler>();
        }
    }
}
