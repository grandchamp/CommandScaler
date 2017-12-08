using CommandScaler.RabbitMQ.Configuration;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Threading.Tasks;

namespace CommandScaler.RabbitMQ.Connection.Contracts
{
    public interface IRabbitConnectionManager
    {
        Task<Result<bool>> Open();

        Task<IModel> CreateChannel();
    }
}
