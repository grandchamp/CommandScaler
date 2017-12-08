using System;

namespace CommandScaler.Handlers
{
    public interface IHandlerList
    {
        void Add(Type key, IBaseCommandHandler commandHandler);
        IBaseCommandHandler Get(Type key);
    }
}
