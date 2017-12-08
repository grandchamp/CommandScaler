using CommandScaler.Bus.Contracts;
using CommandScaler.RabbitMQ.Connection.Contracts;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading.Tasks;

namespace CommandScaler.RabbitMQ.Manager
{
    public class RabbitBus : IBus
    {
        public const string QUEUE_NAME = "commandscaler.bus";

        private readonly IRabbitConnectionManager _connectionManager;
        private readonly IModel _channel;
        private readonly ILogger<RabbitBus> _log;
        public RabbitBus(IRabbitConnectionManager connectionManager, ILogger<RabbitBus> log)
        {
            _connectionManager = connectionManager;
            _log = log;

            _channel = connectionManager.CreateChannel().GetAwaiter().GetResult();
        }

        public Task FireAndForget(ICommand<Unit> command)
        {
            try
            {
                CreateQueueIfNotExists(QUEUE_NAME);

                var request = new Request { Command = command, CommandReturnType = typeof(Unit).FullName, CommandType = command.GetType().FullName };

                var json = JsonConvert.SerializeObject(request);

                _channel.BasicPublish(exchange: "", routingKey: QUEUE_NAME, basicProperties: _channel.CreateBasicProperties(), body: json.Serialize());

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, ex.Message);

                throw;
            }
        }

        public async Task<TResult> Send<TResult>(ICommand<TResult> command)
        {
            try
            {
                CreateQueueIfNotExists(QUEUE_NAME);

                var replyQueueName = _channel.QueueDeclare().QueueName;
                var consumer = new EventingBasicConsumer(_channel);
                var props = _channel.CreateBasicProperties();

                var correlationId = Guid.NewGuid().ToString();
                props.CorrelationId = correlationId;
                props.ReplyTo = replyQueueName;

                var tcs = new TaskCompletionSource<TResult>();

                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    if (ea.BasicProperties.CorrelationId == correlationId)
                    {
                        var result = body.Deserialize<TResult>();

                        tcs.SetResult(result);
                    }
                };

                var request = new Request { Command = command, CommandReturnType = typeof(TResult).FullName, CommandType = command.GetType().FullName };

                var json = JsonConvert.SerializeObject(request, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });

                _channel.BasicPublish(exchange: "", routingKey: QUEUE_NAME, basicProperties: props, body: json.Serialize());

                _channel.BasicConsume(consumer: consumer, queue: replyQueueName, autoAck: true);

                return await tcs.Task;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, ex.Message);

                throw;
            }
        }

        private void CreateQueueIfNotExists(string queueName) => _channel.QueueDeclare(queueName, false, false, true, null);
    }
}
