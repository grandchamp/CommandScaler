using CSharpFunctionalExtensions;
using RabbitMqNext;
using System.Threading.Tasks;

namespace CommandScaler.RabbitMQ.Connection.Contracts
{
    public interface IRabbitConnectionManager
    {
        Task<Result<bool>> Open();

        Task<IChannel> CreateChannel();
    }
}
