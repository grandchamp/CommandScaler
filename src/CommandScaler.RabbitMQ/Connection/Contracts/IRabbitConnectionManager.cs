using CSharpFunctionalExtensions;
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
