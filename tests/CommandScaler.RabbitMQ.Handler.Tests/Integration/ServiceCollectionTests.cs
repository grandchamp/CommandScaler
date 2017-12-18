using CommandScaler.Bus.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
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

        [Fact]
        public async Task CommandHandlerCanHandleParallelCommand()
        {
            const int iterations = 30;

            var taskList = new List<Task>(iterations);
            var concurrentResult = new ConcurrentBag<string>();

            var expected = new List<string>(iterations);
            for (int i = 0; i < iterations; i++)
                expected.Add($"{i.ToString().PadLeft(4, '0')} DependencyString");

            Parallel.For(0, iterations, i =>
            {
                taskList.Add(Task.Run(async () =>
                {
                    var result = await _client.GetStringAsync($"/run/2/{i}");
                    concurrentResult.Add(result);
                }));
            });

            await Task.WhenAll(taskList.ToArray());
            Assert.Equal(30, concurrentResult.Count);
            Assert.Equal(expected, concurrentResult.OrderBy(x => x));
        }
    }
}
