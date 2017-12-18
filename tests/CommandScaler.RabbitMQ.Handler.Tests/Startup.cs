using CommandScaler.Bus.Contracts;
using CommandScaler.RabbitMQ.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

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

            services.AddSingleton<TestDependency>();

            services.AddRouting();

            services.AddCommandScaler(new[] { typeof(TestCommand1).Assembly })
                    .AddRabbitMQCommandScaler()
                    .ConfigureRabbitMQHandler();
        }

        public async void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.Map("/run/1", HandleRunTestCommand1);

            var routeBuilder = new RouteBuilder(app);

            routeBuilder.MapGet("run/2/{value}", HandleRunTestCommand2);

            var routes = routeBuilder.Build();
            app.UseRouter(routes);

            await app.StartRabbitMQHandler();
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

        private static async Task HandleRunTestCommand2(HttpContext context)
        {
            var value = context.GetRouteValue("value").ToString();

            var bus = context.RequestServices.GetService<IBus>();
            var result = await bus.Send(new TestCommand2 { ValueToReturn = value.PadLeft(4, '0') });

            await context.Response.WriteAsync(result);
        }
    }
}
