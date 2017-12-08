using CommandScaler.Bus.Contracts;
using CommandScaler.Handlers;
using CommandScaler.RabbitMQ.Connection.Contracts;
using Microsoft.Extensions.DependencyInjection;
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
        public RabbitGenericHandlerTests()
        {
            var logger = Substitute.For<ILogger<RabbitGenericHandler>>();
            var connectionManager = Substitute.For<IRabbitConnectionManager>();
            var model = Substitute.For<IModel>();
            connectionManager.CreateChannel().Returns(model);

            var bus = Substitute.For<IBus>();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient(typeof(TestCommandHandler1).GetCommandHandlerInterface(), typeof(TestCommandHandler1));
            serviceCollection.AddTransient(typeof(TestCommandHandler2).GetCommandHandlerInterface(), typeof(TestCommandHandler2));

            var handlerFactory = new HandlerFactory(serviceCollection.BuildServiceProvider());

            _rabbitGenericHandler = new RabbitGenericHandler(handlerFactory, logger, connectionManager);
        }

        [Fact]
        public async void CanCreateHandler()
        {
            await _rabbitGenericHandler.CreateHandler();
        }
    }
}
