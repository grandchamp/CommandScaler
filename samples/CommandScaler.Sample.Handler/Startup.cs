using CommandScaler.RabbitMQ.Configuration;
using CommandScaler.Sample.Core.Commands;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CommandScaler.Sample.Handler
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<RabbitConfiguration>(Configuration.GetSection("RabbitMQ"));

            services.AddCommandScaler(new[] { typeof(DelayedCommand).Assembly })
                    .AddRabbitMQCommandScaler()
                    .ConfigureRabbitMQHandler();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            await app.StartRabbitMQHandler();
        }
    }
}
