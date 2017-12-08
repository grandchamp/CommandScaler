using System;
using System.Collections.Concurrent;

namespace CommandScaler.Handlers
{
    public class HandlerList : IHandlerList
    {
        private readonly ConcurrentDictionary<Type, IBaseCommandHandler> _handlers;
        public HandlerList()
        {
            _handlers = new ConcurrentDictionary<Type, IBaseCommandHandler>();
        }

        public void Add(Type key, IBaseCommandHandler commandHandler) => _handlers.GetOrAdd(key, commandHandler);

        public IBaseCommandHandler Get(Type key) => _handlers[key];
    }
}
