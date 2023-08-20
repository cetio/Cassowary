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
using System.Runtime.CompilerServices;

namespace Cassowary.Reflection.Exposers
{
    public sealed class TypeExposer
    {
        internal object _value;
        internal InstanceExposer _instanceExposer;

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public TypeExposer(Type type, params object[] arguments)
        {
            _value = Activator.CreateInstance(type, arguments);
            _instanceExposer = new InstanceExposer(type);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public TypeExposer(string typeName, params object[] arguments)
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            Type type = TypeFactory.ResolveType(typeName, true);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

#pragma warning disable CS8604 // Possible null reference argument.
            _value = Activator.CreateInstance(type, arguments);
#pragma warning restore CS8604 // Possible null reference argument.
            _instanceExposer = new InstanceExposer(type);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public TypeExposer(string assemblyName, string typeName, params object[] arguments)
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            Type type = TypeFactory.ResolveType(assemblyName, typeName, true);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

#pragma warning disable CS8604 // Possible null reference argument.
            _value = Activator.CreateInstance(type, arguments);
#pragma warning restore CS8604 // Possible null reference argument.
            _instanceExposer = new InstanceExposer(type);
        }

        public object Object
        {
            get
            {
                return _value;
            }
            set
            {
                if (value is not TypeExposer exposer)
                    throw new ArgumentException("Cannot set value of a TypeExposer to a non-TypeExposer maintained value");

                _value = exposer.Object;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public object? Invoke(string methodName, params object?[] parameters)
        {
            return _instanceExposer.Invoke(methodName, _value, parameters);
        }
    }
}
