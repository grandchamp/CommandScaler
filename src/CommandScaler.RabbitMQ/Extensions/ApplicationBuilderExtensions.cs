using CommandScaler.RabbitMQ.Connection.Contracts;
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
    }
}
