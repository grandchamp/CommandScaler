using CommandScaler.Handlers;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CommandScaler
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCommandScaler(this IServiceCollection serviceCollection, IEnumerable<Assembly> commandHandlersAssemblies)
        {
            var commandHandlerBaseType = typeof(ICommandHandler<,>);

            foreach (var type in commandHandlersAssemblies.SelectMany(x => x.ExportedTypes).Where(x => x.IsAssignableToGenericType(commandHandlerBaseType)))
                serviceCollection.AddTransient(type.GetCommandHandlerInterface(), type);

            serviceCollection.AddSingleton<IHandlerFactory, HandlerFactory>();

            return serviceCollection;
        }
    }
}
