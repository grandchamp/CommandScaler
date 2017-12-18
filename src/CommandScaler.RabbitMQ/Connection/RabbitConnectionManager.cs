using CommandScaler.RabbitMQ.Configuration;
using CommandScaler.RabbitMQ.Connection.Contracts;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using RabbitMqNext;
using System;
using System.Threading.Tasks;

namespace CommandScaler.RabbitMQ.Connection
{
    public class RabbitConnectionManager : IRabbitConnectionManager
    {
        private IConnection _connection;
        private readonly IOptions<RabbitConfiguration> _rabbitConfiguration;
        private readonly ILogger<RabbitConnectionManager> _log;
        public RabbitConnectionManager(IOptions<RabbitConfiguration> rabbitConfiguration, ILogger<RabbitConnectionManager> log)
        {
            _rabbitConfiguration = rabbitConfiguration;
            _log = log;
        }

        public async Task<Result<bool>> Open()
        {
            try
            {
                if (_connection?.IsClosed == null || _connection?.IsClosed == false)
                {
                    await Policy.Handle<Exception>()
                                .WaitAndRetryAsync(3, x => TimeSpan.FromSeconds(10))
                                .ExecuteAsync(async () => _connection = await ConnectionFactory.Connect(hostname: _rabbitConfiguration.Value.Host,
                                                                                                        username: _rabbitConfiguration.Value.User,
                                                                                                        password: _rabbitConfiguration.Value.Password,
                                                                                                        maxChannels: 10000,
                                                                                                        recoverySettings: new AutoRecoverySettings { Enabled = true }));
                }

                return Result.Ok(true);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, ex.Message);

                return Result.Fail<bool>($"There was an error trying to open connection to RabbitMQ. {ex.Message}");
            }
        }

        public async Task<IChannel> CreateChannel()
        {
            if (_connection == null)
                await Open();

            return await _connection.CreateChannel();
        }
    }
}
