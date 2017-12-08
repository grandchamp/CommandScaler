using CommandScaler.Handlers;
using System.Threading.Tasks;

namespace CommandScaler.RabbitMQ.Handler.Tests
{
    public class TestCommand1 : ICommand<int>
    {
        public int ValueToReturn { get; set; }
    }

    public class TestCommandHandler1 : ICommandHandler<TestCommand1, int>
    {
        public async Task<int> Handle(TestCommand1 command)
        {
            await Task.Delay(4000);

            return command.ValueToReturn;
        }
    }

    public class TestCommand2 : ICommand<string>
    {
        public string ValueToReturn { get; set; }
    }

    public class TestCommandHandler2 : ICommandHandler<TestCommand2, string>
    {
        public async Task<string> Handle(TestCommand2 command) => command.ValueToReturn;
    }
}
