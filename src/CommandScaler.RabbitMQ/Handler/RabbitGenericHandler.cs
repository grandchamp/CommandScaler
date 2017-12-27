using CommandScaler.Handlers;
using CommandScaler.RabbitMQ.Connection.Contracts;
using CommandScaler.RabbitMQ.Manager;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading.Tasks;

namespace CommandScaler.RabbitMQ.Handler
{
    public class RabbitGenericHandler
    {
        private readonly IHandlerFactory _handlerFactory;
        private readonly ILogger<RabbitGenericHandler> _log;
        private readonly IRabbitConnectionManager _connectionManager;
        public RabbitGenericHandler(IHandlerFactory handlerFactory, ILogger<RabbitGenericHandler> log, IRabbitConnectionManager connectionManager)
        {
            _handlerFactory = handlerFactory;
            _log = log;
            _connectionManager = connectionManager;
        }

        public async Task CreateHandler()
        {
            var channel = await _connectionManager.CreateChannel();

            channel.QueueDeclare(RabbitBus.QUEUE_NAME, false, false, true, null);

            var consumer = new EventingBasicConsumer(channel);
            channel.BasicQos(0, 1, false);
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

                    var handler = (dynamic)_handlerFactory.Get(requestCommandHandlerType);
                    var handleResult = (dynamic)handler.Handle((dynamic)request.Command);

                    await handleResult;

                    response = handleResult.Result;
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
        }
    }
}
