namespace CommandScaler.Sample.Core.Commands
{
    public class DelayedCommand : ICommand<string>
    {
        public string Message { get; set; }
    }
}
