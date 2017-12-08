namespace CommandScaler
{
    public class Request
    {
        public string CommandReturnType { get; set; }
        public string CommandType { get; set; }

        public ICommandBase Command { get; set; }
    }
}
