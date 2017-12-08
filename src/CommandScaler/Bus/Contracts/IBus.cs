using System.Threading.Tasks;

namespace CommandScaler.Bus.Contracts
{
    public interface IBus
    {
        Task<TResult> Send<TResult>(ICommand<TResult> command);
        Task FireAndForget(ICommand<Unit> command);
    }
}
