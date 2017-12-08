using CommandScaler.RabbitMQ.Connection.Contracts;
using CommandScaler.RabbitMQ.Handler;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace CommandScaler
{
    public static class ApplicationBuilderExtensions
    {
        public static async Task StartCommandScaler(this IApplicationBuilder app)
        {
            var rabbitConnectionManager = app.ApplicationServices.GetRequiredService<IRabbitConnectionManager>();
            await rabbitConnectionManager.Open();
        }

        public static async Task StartRabbitMQHandler(this IApplicationBuilder app)
        {
            var rabbitConnectionManager = app.ApplicationServices.GetRequiredService<IRabbitConnectionManager>();
            await rabbitConnectionManager.Open();

            var rabbitGenericHandler = app.ApplicationServices.GetService<RabbitGenericHandler>();
            await rabbitGenericHandler.CreateHandler();
        }
    }
}
