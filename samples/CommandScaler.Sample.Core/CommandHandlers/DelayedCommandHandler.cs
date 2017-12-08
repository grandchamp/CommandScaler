using CommandScaler.Handlers;
using CommandScaler.Sample.Core.Commands;
using System;
using System.Threading.Tasks;

namespace CommandScaler.Sample.Core.CommandHandlers
{
    public class DelayedCommandHandler : ICommandHandler<DelayedCommand, string>
    {
        public async Task<string> Handle(DelayedCommand command)
        {
            var randomDelay = new Random().Next(1000, 10000);
            await Task.Delay(randomDelay);

            return $"I've delayed {randomDelay}ms. My return message is: {command.Message}";
        }
    }
}
