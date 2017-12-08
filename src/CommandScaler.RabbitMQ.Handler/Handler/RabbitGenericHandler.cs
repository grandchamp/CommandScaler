﻿using CommandScaler.Bus.Contracts;
using CommandScaler.Handlers;
using CommandScaler.RabbitMQ.Connection.Contracts;
using CommandScaler.RabbitMQ.Manager;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommandScaler.RabbitMQ.Handler.Handler
{
    public class RabbitGenericHandler
    {
        private readonly IHandlerList _handlerList;
        private readonly ILogger<RabbitGenericHandler> _log;
        private readonly IRabbitConnectionManager _connectionManager;
        private readonly IBus _bus;
        public RabbitGenericHandler(IHandlerList handlerList, ILogger<RabbitGenericHandler> log, IRabbitConnectionManager connectionManager, IBus bus)
        {
            _handlerList = handlerList;
            _log = log;
            _connectionManager = connectionManager;
            _bus = bus;
        }

        public Task CreateHandler()
        {
            var channel = _connectionManager.CreateChannel().GetAwaiter().GetResult();

            channel.QueueDeclare(RabbitBus.QUEUE_NAME, false, false, true, null);

            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(queue: RabbitBus.QUEUE_NAME, autoAck: false, consumer: consumer);

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body;
                var props = ea.BasicProperties;
                var replyProps = channel.CreateBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;

                object response = new object();

                try
                {
                    var json = body.Deserialize<string>();

                    var request = JsonConvert.DeserializeObject<Request>(json, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                    var genericCommandHandlerType = typeof(ICommandHandler<,>);
                    var requestCommandHandlerType = genericCommandHandlerType.MakeGenericType(request.Command.GetType(), Type.GetType(request.CommandReturnType));

                    var handler = _handlerList.Get(requestCommandHandlerType);

                    var handleMethodInfo = handler.GetType().GetMethod("Handle");
                    var handleMethodParameters = handleMethodInfo.GetParameters();

                    var handle = (Task)handleMethodInfo.Invoke(handler, new object[] { request.Command });
                    await handle;

                    response = handle.GetType().GetProperty("Result").GetValue(handle);
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, ex.Message);
                }
                finally
                {
                    var responseBytes = response.Serialize();

                    channel.BasicPublish(exchange: "", routingKey: props.ReplyTo,
                                         basicProperties: replyProps, body: responseBytes);

                    channel.BasicAck(deliveryTag: ea.DeliveryTag,
                                     multiple: false);
                }
            };

            return Task.CompletedTask;
        }
    }
}
