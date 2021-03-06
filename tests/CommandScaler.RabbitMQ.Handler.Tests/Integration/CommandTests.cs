﻿using CommandScaler.Bus.Contracts;
using CommandScaler.Handlers;
using CommandScaler.RabbitMQ.Configuration;
using CommandScaler.RabbitMQ.Connection;
using CommandScaler.RabbitMQ.Manager;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Threading.Tasks;
using Xunit;

namespace CommandScaler.RabbitMQ.Handler.Tests.Integration
{
    public class CommandTests
    {
        private readonly IBus _bus;
        public CommandTests()
        {
            var rabbitGenericHandlerLogger = Substitute.For<ILogger<RabbitGenericHandler>>();
            var rabbitConnectionManagerLogger = Substitute.For<ILogger<RabbitConnectionManager>>();
            var rabbitBusLogger = Substitute.For<ILogger<RabbitBus>>();

            var rabbitOptions = Substitute.For<IOptions<RabbitConfiguration>>();
            rabbitOptions.Value.Returns(x => new RabbitConfiguration
            {
                Host = "10.1.3.164",
                User = "admin",
                Password = "123456"
            });

            var connectionManager = new RabbitConnectionManager(rabbitOptions, rabbitConnectionManagerLogger);
            connectionManager.Open().GetAwaiter().GetResult();
            var channel = connectionManager.CreateChannel().GetAwaiter().GetResult();

            _bus = new RabbitBus(connectionManager, rabbitBusLogger);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient(typeof(TestCommandHandler1).GetCommandHandlerInterface(), typeof(TestCommandHandler1));
            serviceCollection.AddTransient(typeof(TestCommandHandler2).GetCommandHandlerInterface(), typeof(TestCommandHandler2));

            var handlerFactory = new HandlerFactory(serviceCollection.BuildServiceProvider());

            var rabbitGenericHandler = new RabbitGenericHandler(handlerFactory, rabbitGenericHandlerLogger, connectionManager);
            rabbitGenericHandler.CreateHandler().GetAwaiter().GetResult();
        }

        [Fact]
        public async Task CommandReturnsCorrectValue()
        {
            var expectedResult = 1;
            var result = await _bus.Send(new TestCommand1 { ValueToReturn = expectedResult });

            Assert.Equal(expectedResult, result);
        }
    }
}
