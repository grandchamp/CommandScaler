using System;

namespace CommandScaler.Handlers
{
    public class HandlerFactory : IHandlerFactory
    {
        private readonly IServiceProvider _serviceProvider;
        public HandlerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IBaseCommandHandler Get(Type key) => (IBaseCommandHandler)_serviceProvider.GetService(key);
    }
}
