using CommandScaler.Handlers;
using System;

namespace CommandScaler
{
    public static class ReflectionExtensions
    {
        public static Type GetCommandHandlerInterface(this Type type)
        {
            var commandHandlerDefaultType = typeof(ICommandHandler<,>);
            Type returnType = null;

            if (type.IsAssignableToGenericType(commandHandlerDefaultType))
                returnType = type.GetAssignableGenericType(commandHandlerDefaultType);

            if (returnType == null)
                throw new Exception("Could not get ICommandHandler<> from this type.");

            return returnType;
        }

        public static bool IsAssignableToGenericType(this Type givenType, Type genericType)
        {
            var interfaceTypes = givenType.GetInterfaces();

            foreach (var it in interfaceTypes)
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                    return true;

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
                return true;

            Type baseType = givenType.BaseType;
            if (baseType == null) return false;

            return IsAssignableToGenericType(baseType, genericType);
        }

        public static Type GetAssignableGenericType(this Type givenType, Type genericType)
        {
            var interfaceTypes = givenType.GetInterfaces();

            foreach (var it in interfaceTypes)
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                    return it;

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
                return genericType;

            Type baseType = givenType.BaseType;
            if (baseType == null)
                return null;

            return GetAssignableGenericType(baseType, genericType);
        }
    }
}
