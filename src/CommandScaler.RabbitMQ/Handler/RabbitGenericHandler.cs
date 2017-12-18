using CommandScaler.Handlers;
using CommandScaler.RabbitMQ.Connection.Contracts;
using CommandScaler.RabbitMQ.Manager;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMqNext;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CommandScaler.RabbitMQ.Handler
{
    public class RabbitGenericHandler
    {
        private readonly IHandlerFactory _handlerFactory;
        private readonly ILogger<RabbitGenericHandler> _log;
        private readonly IRabbitConnectionManager _connectionManager;
        private IChannel _channel;
        public RabbitGenericHandler(IHandlerFactory handlerFactory, ILogger<RabbitGenericHandler> log, IRabbitConnectionManager connectionManager)
        {
            _handlerFactory = handlerFactory;
            _log = log;
            _connectionManager = connectionManager;
        }

        public async Task CreateHandler()
        {
            _channel = await _connectionManager.CreateChannel();

            await _channel.QueueDeclare(queue: RabbitBus.QUEUE_NAME, passive: false, durable: true, exclusive: false, autoDelete: false,
                                        arguments: null, waitConfirmation: true);

            _channel.AddErrorCallback(error =>
            {
                return Task.CompletedTask;
            });

            //await _channel.BasicQos(0, 1, false);
            await _channel.BasicConsume(mode: ConsumeMode.SerializedWithBufferCopy, consumer: async (ea) =>
            {
                _log.LogInformation($"Running Handler on: {Thread.CurrentThread.ManagedThreadId}");
                _log.LogInformation($"Received command.");

                var body = new byte[ea.bodySize];
                await ea.stream.ReadAsync(body, 0, ea.bodySize);

                var props = ea.properties;
                var replyProps = _channel.RentBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;

                object response = new object();

                try
                {
                    var json = body.Deserialize<string>();

                    var request = JsonConvert.DeserializeObject<Request>(json, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                    var genericCommandHandlerType = typeof(ICommandHandler<,>);
                    var requestCommandHandlerType = genericCommandHandlerType.MakeGenericType(request.Command.GetType(), Type.GetType(request.CommandReturnType));

                    var handler = _handlerFactory.Get(requestCommandHandlerType);

                    var handleMethodInfo = handler.GetType().GetMethod("Handle");

                    var handleMethod = (Task)handleMethodInfo.Invoke(handler, new object[] { request.Command });

                    await handleMethod;

                    response = handleMethod.GetType().GetProperty("Result").GetValue(handleMethod);
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, ex.Message);
                }
                finally
                {
                    var responseBytes = response.Serialize();

                    _channel.BasicPublishFast(exchange: "", routingKey: props.ReplyTo,
                                             properties: replyProps, buffer: responseBytes);

                    //_channel.BasicAck(deliveryTag: ea.deliveryTag,
                    //                 multiple: false);
                }
            }, queue: RabbitBus.QUEUE_NAME, consumerTag: "", withoutAcks: true, exclusive: false, arguments: null, waitConfirmation: true);
        }
    }
}
