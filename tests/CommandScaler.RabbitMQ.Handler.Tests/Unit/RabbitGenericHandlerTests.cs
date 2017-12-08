using CommandScaler.Bus.Contracts;
using CommandScaler.Handlers;
using CommandScaler.RabbitMQ.Connection.Contracts;
using CommandScaler.RabbitMQ.Handler.Handler;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;
using Xunit;

namespace CommandScaler.RabbitMQ.Handler.Tests
{
    public class RabbitGenericHandlerTests
    {
        private readonly RabbitGenericHandler _rabbitGenericHandler;
        private readonly ConcurrentDictionary<Type, IBaseCommandHandler> _handlerDictionary;
        public RabbitGenericHandlerTests()
        {
            var logger = Substitute.For<ILogger<RabbitGenericHandler>>();
            var connectionManager = Substitute.For<IRabbitConnectionManager>();
            var model = Substitute.For<IModel>();
            connectionManager.CreateChannel().Returns(model);

            var bus = Substitute.For<IBus>();

            var handlerList = new HandlerList();
            handlerList.Add(typeof(TestCommandHandler1).GetCommandHandlerInterface(), new TestCommandHandler1());
            handlerList.Add(typeof(TestCommandHandler2).GetCommandHandlerInterface(), new TestCommandHandler2());

            _rabbitGenericHandler = new RabbitGenericHandler(handlerList, logger, connectionManager, bus);
        }

        [Fact]
        public async void CanCreateHandler()
        {
            await _rabbitGenericHandler.CreateHandler();
        }
    }
}
