using System;
using System.Reflection;

namespace Pagr.Extensions
{
    // The default GetMethod doesn't allow for generic methods which means
    // custom filters for different sources can't share the same name.
    // https://stackoverflow.com/questions/4035719/getmethod-for-generic-method
    public static class MethodInfoExtended
    {
        /// <summary>
        /// Search for a method by name and parameter types.  
        /// Unlike GetMethod(), does 'loose' matching on generic
        /// parameter types, and searches base interfaces.
        /// </summary>
        /// <exception cref="AmbiguousMatchException"/>
        public static MethodInfo GetMethodExt(this Type thisType, string name, Type firstType)
        {
            return GetMethodExt(thisType, name, BindingFlags.Instance
                                                | BindingFlags.Static
                                                | BindingFlags.Public
                                                | BindingFlags.NonPublic
                                                | BindingFlags.FlattenHierarchy,
                                firstType);
        }

        /// <summary>
        /// Search for a method by name, parameter types, and binding flags.  
        /// Unlike GetMethod(), does 'loose' matching on generic
        /// parameter types, and searches base interfaces.
        /// </summary>
        /// <exception cref="AmbiguousMatchException"/>
        public static MethodInfo GetMethodExt(this Type thisType,
                                                string name,
                                                BindingFlags bindingFlags,
                                                Type firstType)
        {
            MethodInfo matchingMethod = null;

            // Check all methods with the specified name, including in base classes
            GetMethodExt(ref matchingMethod, thisType, name, bindingFlags, firstType);

            // If we're searching an interface, we have to manually search base interfaces
            if (matchingMethod != null || !thisType.IsInterface)
            {
                return matchingMethod;
            }

            foreach (var interfaceType in thisType.GetInterfaces())
            {
                GetMethodExt(ref matchingMethod, interfaceType, name, bindingFlags, firstType);
            }

            return matchingMethod;
        }

        private static void GetMethodExt(ref MethodInfo matchingMethod, Type type, string name, BindingFlags bindingFlags, Type firstType)
        {
            // Check all methods with the specified name, including in base classes
            foreach (var memberInfo in type.GetMember(name, MemberTypes.Method, bindingFlags))
            {
                var methodInfo = (MethodInfo)memberInfo;

                // Check that the parameter counts and types match, 
                // with 'loose' matching on generic parameters
                var parameterInfos = methodInfo.GetParameters();

                if (!parameterInfos[0].ParameterType.IsSimilarType(firstType))
                {
                    continue;
                }

                if (matchingMethod == null)
                {
                    matchingMethod = methodInfo;
                }
                else
                {
                    throw new AmbiguousMatchException("More than one matching method found!");
                }
            }
        }

        /// <summary>
        /// Special type used to match any generic parameter type in GetMethodExt().
        /// </summary>
        private class T
        { }

        /// <summary>
        /// Determines if the two types are either identical, or are both generic 
        /// parameters or generic types with generic parameters in the same
        ///  locations (generic parameters match any other generic parameter,
        /// but NOT concrete types).
        /// </summary>
        private static bool IsSimilarType(this Type thisType, Type type)
        {
            // Ignore any 'ref' types
            if (thisType.IsByRef)
            {
                thisType = thisType.GetElementType();
            }

            if (type.IsByRef)
            {
                type = type.GetElementType();
            }

            // Handle array types
            if (thisType.IsArray && type.IsArray)
            {
                return thisType.GetElementType().IsSimilarType(type.GetElementType());
            }

            // If the types are identical, or they're both generic parameters 
            // or the special 'T' type, treat as a match
            if (thisType == type || 
                (thisType.IsGenericParameter || thisType == typeof(T)) && (type.IsGenericParameter || type == typeof(T)))
            {
                return true;
            }

            // Handle any generic arguments
            if (thisType.IsGenericType && type.IsGenericType)
            {
                var thisArguments = thisType.GetGenericArguments();
                var arguments = type.GetGenericArguments();
                if (thisArguments.Length != arguments.Length)
                {
                    return false;
                }

                for (var i = 0; i < thisArguments.Length; ++i)
                {
                    if (!thisArguments[i].IsSimilarType(arguments[i]))
                    {
                        return false;
                    }
                }
                return true;
            }

            return false;
        }
    }
}
