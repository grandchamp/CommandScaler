using CommandScaler.Bus.Contracts;
using CommandScaler.RabbitMQ.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CommandScaler.RabbitMQ.Handler.Tests
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<RabbitConfiguration>(Configuration.GetSection("RabbitMQ"));

            services.AddCommandScaler(new[] { typeof(TestCommand1).Assembly })
                    .AddRabbitMQCommandScaler()
                    .ConfigureRabbitMQHandler();
        }

        public async void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            await app.StartRabbitMQHandler();

            app.Map("/run/1", HandleRunTestCommand1);
            app.Map("/run/2", HandleRunTestCommand2);
        }

        private static void HandleRunTestCommand1(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                var bus = app.ApplicationServices.GetService<IBus>();
                var result = await bus.Send(new TestCommand1 { ValueToReturn = 1 });

                await context.Response.WriteAsync(result.ToString());
            });
        }

        private static void HandleRunTestCommand2(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                var bus = app.ApplicationServices.GetService<IBus>();
                var result = await bus.Send(new TestCommand2 { ValueToReturn = "a" });

                await context.Response.WriteAsync(result);
            });
        }
    }
}
