using System;

namespace CommandScaler.Handlers
{
    public interface IHandlerFactory
    {
        IBaseCommandHandler Get(Type key);
    }
}
