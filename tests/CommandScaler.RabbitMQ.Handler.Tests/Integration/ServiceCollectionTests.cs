using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CommandScaler.RabbitMQ.Handler.Tests.Integration
{
    public class ServiceCollectionTests
    {
        private readonly TestServer _testServer;
        private readonly HttpClient _client;
        public ServiceCollectionTests()
        {
            var configuration = new ConfigurationBuilder()
                                      .SetBasePath(Directory.GetCurrentDirectory())
                                      .AddJsonFile($"rabbitconfiguration.json", optional: false, reloadOnChange: true)
                                      .AddEnvironmentVariables()
                                      .Build();

            _testServer = new TestServer(new WebHostBuilder()
                                              .ConfigureLogging(x => x.AddConsole())
                                              .UseConfiguration(configuration)
                                              .UseStartup<Startup>());

            _client = _testServer.CreateClient();
        }

        [Fact]
        public async Task TestCommand1ReturnsCorrectValue()
        {
            var result = await _client.GetStringAsync("/run/1");

            Assert.Equal("1", result);
        }

        [Fact]
        public async Task TestCommand2WithDIDependencyReturnsCorrectValue()
        {
            var result = await _client.GetStringAsync("/run/2");

            Assert.Equal("a DependencyString", result);
        }
    }
}
