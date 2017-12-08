﻿using CommandScaler.RabbitMQ.Configuration;
using CommandScaler.RabbitMQ.Connection.Contracts;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
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

        public Task<Result<bool>> Open()
        {
            try
            {
                if (_connection?.IsOpen == null || _connection?.IsOpen == false)
                {
                    var rabbitFactory = new ConnectionFactory
                    {
                        HostName = _rabbitConfiguration.Value.Host,
                        UserName = _rabbitConfiguration.Value.User,
                        Password = _rabbitConfiguration.Value.Password
                    };

                    _connection = rabbitFactory.CreateConnection();
                }

                return Task.FromResult(Result.Ok(true));
            }
            catch (Exception ex)
            {
                _log.LogError(ex, ex.Message);

                return Task.FromResult(Result.Fail<bool>($"There was an error trying to open connection to RabbitMQ. {ex.Message}"));
            }
        }

        public Task<IModel> CreateChannel() => Task.FromResult(_connection.CreateModel());
    }
}
