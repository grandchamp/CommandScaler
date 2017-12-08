using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.Extensions.Configuration;
using Xunit;
using System.Threading.Tasks;
using CommandScaler.Bus.Contracts;
using System.Net.Http;

namespace CommandScaler.RabbitMQ.Handler.Tests.Integration
{
    public class ServiceCollectionTests
    {
        private readonly TestServer _testServer;
        private readonly HttpClient _client;
        private readonly IBus _bus;
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
        public async Task TestCommand2ReturnsCorrectValue()
        {
            var result = await _client.GetStringAsync("/run/2");

            Assert.Equal("a", result);
        }
    }
}
