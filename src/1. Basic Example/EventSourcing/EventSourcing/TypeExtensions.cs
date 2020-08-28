namespace EventSourcing
{
    using System;
    using System.Linq;
    using System.Reflection;

    internal static class TypeExtensions
    {
        public static MethodInfo GetAggregateUpdateStateMethodForMessage(this Type type, string methodName, Type messageType)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            MethodInfo[] updateStateMethodInfos = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo[] selectedUpdateStateMethodInfos = updateStateMethodInfos.Where(
                mi => mi.GetParameters().Length == 1 &&
                mi.GetParameters()[0].ParameterType.IsAssignableFrom(messageType) &&
                mi.Name == methodName).ToArray();
            if (selectedUpdateStateMethodInfos.Length > 1)
            {
                string exceptionMessage = string.Format("Found more than one method named '{0}' for type '{1}' that accepted message type '{2}', so could not determine which method to call.", methodName, type.FullName, messageType.FullName);
                throw new ApplicationException(exceptionMessage);
            }

            return selectedUpdateStateMethodInfos.FirstOrDefault();
        }
    }
}
