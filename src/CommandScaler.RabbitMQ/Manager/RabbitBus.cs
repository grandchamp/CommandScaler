using CommandScaler.Bus.Contracts;
using CommandScaler.RabbitMQ.Connection.Contracts;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CommandScaler.RabbitMQ.Manager
{
    public class RabbitBus : IBus, IDisposable
    {
        public const string QUEUE_NAME = "commandscaler.bus";
        public const string DIRECT_REPLYTO_QUEUE_NAME = "amq.rabbitmq.reply-to";

        private bool disposed;

        private readonly IRabbitConnectionManager _connectionManager;
        private readonly IModel _channel;
        private readonly ILogger<RabbitBus> _log;
        public RabbitBus(IRabbitConnectionManager connectionManager, ILogger<RabbitBus> log)
        {
            _connectionManager = connectionManager;
            _log = log;

            _channel = connectionManager.CreateChannel().GetAwaiter().GetResult();
            disposed = false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _log.LogInformation("Dispose called.");

            if (!disposed && disposing)
                _channel?.Dispose();

            disposed = true;
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

        public Task<TResult> Send<TResult>(ICommand<TResult> command)
        {
            try
            {
                _log.LogInformation($"Running on: {Thread.CurrentThread.ManagedThreadId}");

                CreateQueueIfNotExists(QUEUE_NAME);
                
                var consumer = new EventingBasicConsumer(_channel);
                var props = _channel.CreateBasicProperties();
                
                var correlationId = Guid.NewGuid().ToString();
                props.CorrelationId = correlationId;
                props.ReplyTo = DIRECT_REPLYTO_QUEUE_NAME;
                
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
                
                _channel.BasicConsume(consumer: consumer, queue: DIRECT_REPLYTO_QUEUE_NAME, autoAck: true);
                
                _channel.BasicPublish(exchange: "", routingKey: QUEUE_NAME, basicProperties: props, body: json.Serialize());

                return tcs.Task;
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
