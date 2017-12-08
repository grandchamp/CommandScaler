using CommandScaler.Bus.Contracts;
using CommandScaler.Handlers;
using CommandScaler.RabbitMQ.Configuration;
using CommandScaler.RabbitMQ.Connection;
using CommandScaler.RabbitMQ.Handler.Handler;
using CommandScaler.RabbitMQ.Manager;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
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

            var handlerList = new HandlerList();
            handlerList.Add(typeof(TestCommandHandler1).GetCommandHandlerInterface(), new TestCommandHandler1());
            handlerList.Add(typeof(TestCommandHandler2).GetCommandHandlerInterface(), new TestCommandHandler2());

            var rabbitGenericHandler = new RabbitGenericHandler(handlerList, rabbitGenericHandlerLogger, connectionManager, _bus);
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
