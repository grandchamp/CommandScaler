using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace CommandScaler.Sample.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                                   .SetBasePath(Directory.GetCurrentDirectory())
                                   .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true)
                                   .AddEnvironmentVariables()
                                   .Build();

            WebHost.CreateDefaultBuilder(args)
                   .UseConfiguration(configuration)
                   .UseStartup<Startup>()
                   .Build()
                   .Run();
        }
    }
}
