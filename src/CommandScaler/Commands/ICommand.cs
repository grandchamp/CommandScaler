namespace CommandScaler
{
    public interface ICommandBase { }
    public interface ICommand<out TResult> : ICommandBase { }
}
