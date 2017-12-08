using System.Threading.Tasks;

namespace CommandScaler.Handlers
{
    public interface IBaseCommandHandler { }

    public interface ICommandHandler<in TCommand, TCommandResult> : IBaseCommandHandler
        where TCommand : ICommand<TCommandResult>
    {
        Task<TCommandResult> Handle(TCommand command);
    }
}
