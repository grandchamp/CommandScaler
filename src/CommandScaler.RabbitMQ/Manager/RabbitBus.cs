using CommandScaler.Bus.Contracts;
using CommandScaler.RabbitMQ.Connection.Contracts;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMqNext;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CommandScaler.RabbitMQ.Manager
{
    public class RabbitBus : IBus/*, IDisposable*/
    {
        public const string QUEUE_NAME = "commandscaler.bus";
        public const string DIRECT_REPLYTO_QUEUE_NAME = "amq.rabbitmq.reply-to";

        private bool disposed;

        private readonly IRabbitConnectionManager _connectionManager;
        private IChannel _channel;
        private readonly ILogger<RabbitBus> _log;
        public RabbitBus(IRabbitConnectionManager connectionManager, ILogger<RabbitBus> log)
        {
            _connectionManager = connectionManager;
            _log = log;

            _channel = connectionManager.CreateChannel().GetAwaiter().GetResult();
            disposed = false;
        }

        //public void Dispose()
        //{
        //    Dispose(true);
        //    GC.SuppressFinalize(this);
        //}

        //protected virtual void Dispose(bool disposing)
        //{
        //    _log.LogInformation("Dispose called.");

        //    if (!disposed && disposing)
        //        _channel?.Dispose();

        //    disposed = true;
        //}

        public async Task FireAndForget(ICommand<Unit> command)
        {
            try
            {
                await CreateQueueIfNotExists(QUEUE_NAME);

                var request = new Request { Command = command, CommandReturnType = typeof(Unit).FullName, CommandType = command.GetType().FullName };

                var json = JsonConvert.SerializeObject(request);

                await _channel.BasicPublish(exchange: "", routingKey: QUEUE_NAME, properties: _channel.RentBasicProperties(), buffer: json.Serialize());
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
                _log.LogInformation($"Running on: {Thread.CurrentThread.ManagedThreadId}");

                await CreateQueueIfNotExists(QUEUE_NAME);

                var props = _channel.RentBasicProperties();
                props.ReplyTo = DIRECT_REPLYTO_QUEUE_NAME;

                var request = new Request { Command = command, CommandReturnType = typeof(TResult).FullName, CommandType = command.GetType().FullName };

                var json = JsonConvert.SerializeObject(request, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });

                _channel.AddErrorCallback(error =>
                {
                    return Task.CompletedTask;
                });

                var rpcClient = await _channel.CreateRpcHelper(ConsumeMode.ParallelWithBufferCopy, null);
                var rpcResult = await rpcClient.Call("", QUEUE_NAME, props, json.Serialize());

                var buffer = new byte[rpcResult.bodySize];
                await rpcResult.stream.ReadAsync(buffer, 0, rpcResult.bodySize);

                if (rpcResult.properties.CorrelationId == props.CorrelationId)
                {
                    var result = buffer.Deserialize<TResult>();

                    return result;
                }

                return default(TResult);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, ex.Message);

                throw;
            }
        }

        private async Task CreateQueueIfNotExists(string queueName)
        {
            _channel = await _connectionManager.CreateChannel();

            await _channel.QueueDeclare(queue: queueName, passive: false, durable: true, exclusive: false,
                                        autoDelete: false, arguments: null, waitConfirmation: true);
        }
    }
}
