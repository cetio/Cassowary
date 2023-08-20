//              FUCK OOP & FUCK INTERLACED LIBRARIES
//
//            DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//                   Version 2, December 2004
// 
// Copyright (C) 2004 Sam Hocevar<sam@hocevar.net>
//
// Everyone is permitted to copy and distribute verbatim or modified
// copies of this license document, and changing it is allowed as long
// as the name is changed.
//
//           DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//  TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION
//
//  0. You just DO WHAT THE FUCK YOU WANT TO.

using Cassowary.Reflection.Factories;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Cassowary.Reflection.Exposers
{
    public sealed class InstanceExposer
    {
        Dictionary<(string name, Type[] types), MethodInfo> _methodDictionary = new Dictionary<(string name, Type[] types), MethodInfo>();

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public InstanceExposer(Type type)
        {
            foreach (MethodInfo method in type.GetRuntimeMethods())
                _methodDictionary.TryAdd((method.Name, method.GetParameters().Select(x => x.ParameterType).ToArray()), method);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public object? Invoke(string methodName, object? instance, params object?[] parameters)
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            if (!_methodDictionary.TryGetValue((methodName, parameters.Select(x => x.GetType()).ToArray()), out MethodInfo methodInfo))
                throw new ArgumentException($"No overload or method {methodName} exists with the provided parameter types");
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            if (instance != null && methodInfo.IsStatic)
                instance = null;

            Delegate @delegate = DelegateFactory.MakeDelegate(methodInfo, instance);
            return Invoker.FastInvoke(@delegate, parameters);
        }
    }
}
