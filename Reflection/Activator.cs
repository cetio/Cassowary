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
using Unsafe = Cassowary.Intrinsics.Unsafe;

namespace Cassowary.Reflection
{
    public static class Activator
    {
        private static Dictionary<Type, object> _presumptiveCache = new Dictionary<Type, object>();

        /// <summary>
        /// Creates a new instance of the specified type using the specified binder and modifiers.
        /// </summary>
        /// <typeparam name="T">The type of the object to create.</typeparam>
        /// <param name="binder">The binder to use to resolve the constructor.</param>
        /// <param name="modifiers">The modifiers to use for the constructor.</param>
        /// <param name="arguments">The arguments to pass to the constructor.</param>
        /// <returns>The newly created object.</returns>
        /// <remarks>
        /// This method uses the <see cref="DelegateFactory.MakeConstructorDelegate(Binder?, ParameterModifier[], Type[])"/> method to create a delegate to the constructor of the specified type. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static object CreateInstance<T>(Binder? binder, ParameterModifier[]? modifiers, params object[] arguments)
        {
            Type[] argumentTypes = arguments.Select(x => x.GetType()).ToArray();
            return Invoker.FastInvoke(DelegateFactory.MakeConstructorDelegate<T>(binder, modifiers, argumentTypes), arguments)!;
        }

        /// <summary>
        /// Creates a new instance of the specified type using the specified binder.
        /// </summary>
        /// <typeparam name="T">The type of the object to create.</typeparam>
        /// <param name="binder">The binder to use to resolve the constructor.</param>
        /// <param name="arguments">The arguments to pass to the constructor.</param>
        /// <returns>The newly created object.</returns>
        /// <remarks>
        /// This method uses the <see cref="DelegateFactory.MakeConstructorDelegate(Binder?, Type[])"/> method to create a delegate to the constructor of the specified type. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static object CreateInstance<T>(Binder? binder, params object[] arguments)
        {
            Type[] argumentTypes = arguments.Select(x => x.GetType()).ToArray();
            return Invoker.FastInvoke(DelegateFactory.MakeConstructorDelegate<T>(binder, argumentTypes), arguments)!;
        }

        /// <summary>
        /// Creates a new instance of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the object to create.</typeparam>
        /// <param name="arguments">The arguments to pass to the constructor.</param>
        /// <returns>The newly created object.</returns>
        /// <remarks>
        /// This method uses the <see cref="DelegateFactory.MakeConstructorDelegate(Type[])"/> method to create a delegate to the constructor of the specified type. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static object CreateInstance<T>(params object[] arguments)
        {
            Type[] argumentTypes = arguments.Select(x => x.GetType()).ToArray();
            return Invoker.FastInvoke(DelegateFactory.MakeConstructorDelegate<T>(argumentTypes), arguments)!;
        }

        /// <summary>
        /// Creates a new instance of the specified type using the specified binder and modifiers.
        /// </summary>
        /// <param name="type">The type of the object to create.</param>
        /// <param name="binder">The binder to use to resolve the constructor.</param>
        /// <param name="modifiers">The modifiers to use for the constructor.</param>
        /// <param name="arguments">The arguments to pass to the constructor.</param>
        /// <returns>The newly created object.</returns>
        /// <remarks>
        /// This method uses the <see cref="DelegateFactory.MakeConstructorDelegate(Type, Binder?, ParameterModifier[], Type[])"/> method to create a delegate to the constructor of the specified type. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static object CreateInstance(Type type, Binder? binder, ParameterModifier[]? modifiers, params object[] arguments)
        {
            Type[] argumentTypes = arguments.Select(x => x.GetType()).ToArray();
            return Invoker.FastInvoke(DelegateFactory.MakeConstructorDelegate(type, binder, modifiers, argumentTypes), arguments)!;
        }

        /// <summary>
        /// Creates a new instance of the specified type using the specified binder.
        /// </summary>
        /// <param name="type">The type of the object to create.</param>
        /// <param name="binder">The binder to use to resolve the constructor.</param>
        /// <param name="arguments">The arguments to pass to the constructor.</param>
        /// <returns>The newly created object.</returns>
        /// <remarks>
        /// This method uses the <see cref="DelegateFactory.MakeConstructorDelegate(Type, Binder?, Type[])"/> method to create a delegate to the constructor of the specified type. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static object CreateInstance(Type type, Binder? binder, params object[] arguments)
        {
            Type[] argumentTypes = arguments.Select(x => x.GetType()).ToArray();
            return Invoker.FastInvoke(DelegateFactory.MakeConstructorDelegate(type, binder, argumentTypes), arguments)!;
        }

        /// <summary>
        /// Creates a new instance of the specified type.
        /// </summary>
        /// <param name="type">The type of the object to create.</param>
        /// <param name="arguments">The arguments to pass to the constructor.</param>
        /// <returns>The newly created object.</returns>
        /// <remarks>
        /// This method uses the <see cref="DelegateFactory.MakeConstructorDelegate(Type, Type[])"/> method to create a delegate to the constructor of the specified type. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static object CreateInstance(Type type, params object[] arguments)
        {
            Type[] argumentTypes = arguments.Select(x => x.GetType()).ToArray();
            return Invoker.FastInvoke(DelegateFactory.MakeConstructorDelegate(type, argumentTypes), arguments)!;
        }

        /// <summary>
        /// Creates a new instance of the specified type using the specified binder and modifiers.
        /// </summary>
        /// <param name="typeName">The name of the type of the object to create.</param>
        /// <param name="binder">The binder to use to resolve the constructor.</param>
        /// <param name="modifiers">The modifiers to use for the constructor.</param>
        /// <param name="arguments">The arguments to pass to the constructor.</param>
        /// <returns>The newly created object.</returns>
        /// <remarks>
        /// This method uses the <see cref="DelegateFactory.MakeConstructorDelegate(string, Binder?, ParameterModifier[], Type[])"/> method to create a delegate to the constructor of the specified type. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static object CreateInstance(string typeName, Binder? binder, ParameterModifier[]? modifiers, params object[] arguments)
        {
            Type[] argumentTypes = arguments.Select(x => x.GetType()).ToArray();
            return Invoker.FastInvoke(DelegateFactory.MakeConstructorDelegate(typeName, binder, modifiers, argumentTypes), arguments)!;
        }

        /// <summary>
        /// Creates a new instance of the specified type using the specified binder.
        /// </summary>
        /// <param name="typeName">The name of the type of the object to create.</param>
        /// <param name="binder">The binder to use to resolve the constructor.</param>
        /// <param name="arguments">The arguments to pass to the constructor.</param>
        /// <returns>The newly created object.</returns>
        /// <remarks>
        /// This method uses the <see cref="DelegateFactory.MakeConstructorDelegate(string, Binder?, Type[])"/> method to create a delegate to the constructor of the specified type. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static object CreateInstance(string typeName, Binder? binder, params object[] arguments)
        {
            Type[] argumentTypes = arguments.Select(x => x.GetType()).ToArray();
            return Invoker.FastInvoke(DelegateFactory.MakeConstructorDelegate(typeName, binder, argumentTypes), arguments)!;
        }

        /// <summary>
        /// Creates a new instance of the specified type.
        /// </summary>
        /// <param name="typeName">The name of the type of the object to create.</param>
        /// <param name="arguments">The arguments to pass to the constructor.</param>
        /// <returns>The newly created object.</returns>
        /// <remarks>
        /// This method uses the <see cref="DelegateFactory.MakeConstructorDelegate(string, Type[])"/> method to create a delegate to the constructor of the specified type. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static object CreateInstance(string typeName, params object[] arguments)
        {
            Type[] argumentTypes = arguments.Select(x => x.GetType()).ToArray();
            return Invoker.FastInvoke(DelegateFactory.MakeConstructorDelegate(typeName, argumentTypes), arguments)!;
        }

        /// <summary>
        /// Creates a new instance of the specified type.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly that contains the type to create.</param>
        /// <param name="typeName">The name of the type of the object to create.</param>
        /// <param name="binder">The binder to use to resolve the constructor.</param>
        /// <param name="modifiers">The modifiers to use for the constructor.</param>
        /// <param name="arguments">The arguments to pass to the constructor.</param>
        /// <returns>The newly created object.</returns>
        /// <remarks>
        /// This method uses the <see cref="DelegateFactory.MakeConstructorDelegate(string, string, Binder?, ParameterModifier[], Type[])"/> method to create a delegate to the constructor of the specified type. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static object CreateInstance(string assemblyName, string typeName, Binder? binder, ParameterModifier[]? modifiers, params object[] arguments)
        {
            Type[] argumentTypes = arguments.Select(x => x.GetType()).ToArray();
            return Invoker.FastInvoke(DelegateFactory.MakeConstructorDelegate(assemblyName, typeName, binder, modifiers, argumentTypes), arguments)!;
        }

        /// <summary>
        /// Creates a new instance of the specified type.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly that contains the type to create.</param>
        /// <param name="typeName">The name of the type of the object to create.</param>
        /// <param name="binder">The binder to use to resolve the constructor.</param>
        /// <param name="arguments">The arguments to pass to the constructor.</param>
        /// <returns>The newly created object.</returns>
        /// <remarks>
        /// This method uses the <see cref="DelegateFactory.MakeConstructorDelegate(string, string, Binder?, Type[])"/> method to create a delegate to the constructor of the specified type. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static object CreateInstance(string assemblyName, string typeName, Binder? binder, params object[] arguments)
        {
            Type[] argumentTypes = arguments.Select(x => x.GetType()).ToArray();
            return Invoker.FastInvoke(DelegateFactory.MakeConstructorDelegate(assemblyName, typeName, binder, argumentTypes), arguments)!;
        }

        /// <summary>
        /// Creates a new instance of the specified type.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly that contains the type to create.</param>
        /// <param name="typeName">The name of the type of the object to create.</param>
        /// <param name="arguments">The arguments to pass to the constructor.</param>
        /// <returns>The newly created object.</returns>
        /// <remarks>
        /// This method uses the <see cref="DelegateFactory.MakeConstructorDelegate(string, string, Type[])"/> method to create a delegate to the constructor of the specified type. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static object CreateInstance(string assemblyName, string typeName, params object[] arguments)
        {
            Type[] argumentTypes = arguments.Select(x => x.GetType()).ToArray();
            return Invoker.FastInvoke(DelegateFactory.MakeConstructorDelegate(assemblyName, typeName, argumentTypes), arguments)!;
        }

        /// <summary>
        /// Creates an uninitialized instance of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the object to create.</typeparam>
        /// <returns>The uninitialized object.</returns>
        /// <remarks>
        /// This method uses the <see cref="System.Runtime.CompilerServices.Unsafe.CreateUninitializedObject(Type)"/> method to create an uninitialized object of the provided type. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static object CreateUninitializedInstance<T>()
        {
            return Unsafe.CreateUninitializedObject(typeof(T));
        }

        /// <summary>
        /// Creates an uninitialized instance of the specified type.
        /// </summary>
        /// <param name="type">The type of the object to create.</param>
        /// <returns>The uninitialized object.</returns>
        /// <remarks>
        /// This method uses the <see cref="System.Runtime.CompilerServices.Unsafe.CreateUninitializedObject(Type)"/> method to create an uninitialized object of the provided type. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static object CreateUninitializedInstance(Type type)
        {
            return Unsafe.CreateUninitializedObject(type);
        }

        /// <summary>
        /// Creates an uninitialized instance of the specified type.
        /// </summary>
        /// <param name="typeName">The name of the type of the object to create.</param>
        /// <returns>The uninitialized object.</returns>
        /// <remarks>
        /// This method uses the <see cref="System.Runtime.CompilerServices.Unsafe.CreateUninitializedObject(Type)"/> method to create an uninitialized object of the provided type. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static object CreateUninitializedInstance(string typeName)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            return Unsafe.CreateUninitializedObject(TypeFactory.ResolveType(typeName, true));
#pragma warning restore CS8604 // Possible null reference argument.
        }

        /// <summary>
        /// Creates an uninitialized instance of the specified type.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly that contains the type to create.</param>
        /// <param name="typeName">The name of the type of the object to create.</param>
        /// <returns>The uninitialized object.</returns>
        /// <remarks>
        /// This method uses the <see cref="System.Runtime.CompilerServices.Unsafe.CreateUninitializedObject(Type)"/> method to create an uninitialized object of the provided type. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static object CreateUninitializedInstance(string assemblyName, string typeName)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            return Unsafe.CreateUninitializedObject(TypeFactory.ResolveType(assemblyName, typeName, true));
#pragma warning restore CS8604 // Possible null reference argument.
        }

        /// <summary>
        /// Creates a presumptive instance of the specified type, optionally constructing a new instance if it contains references.
        /// </summary>
        /// <typeparam name="T">The type of object to create.</typeparam>
        /// <param name="constructNewIfContainsReferences">Whether to construct a new instance if the type contains references.</param>
        /// <returns>A presumptive instance of the specified type.</returns>
        /// <remarks>
        /// A presumptive instance is a clone of a cached object of the given type, this assumes that the parameterless constructor of the type always returns the same object.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static object CreatePresumptiveInstance<T>(bool constructNewIfContainsReferences = false)
        {
            return CreatePresumptiveInstance(typeof(T), constructNewIfContainsReferences);
        }

        /// <summary>
        /// Creates a presumptive instance of the specified type, optionally constructing a new instance if it contains references.
        /// </summary>
        /// <param name="type">The type of object to create.</param>
        /// <param name="constructNewIfContainsReferences">Whether to construct a new instance if the type contains references.</param>
        /// <returns>A presumptive instance of the specified type.</returns>
        /// <remarks>
        /// A presumptive instance is a clone of a cached object of the given type, this assumes that the parameterless constructor of the type always returns the same object.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static object CreatePresumptiveInstance(Type type, bool constructNewIfContainsReferences = false)
        {
            if (constructNewIfContainsReferences && true)//Runtime.ContainsReferences(type))
                return CreateInstance(type);

            if (_presumptiveCache.TryGetValue(type, out var presumption))
                return CreateClone(presumption);

            object obj = CreateInstance(type);
            _presumptiveCache.Add(type, obj);
            return obj;
        }

        /// <summary>
        /// Creates a presumptive instance of the specified type by name, optionally constructing a new instance if it contains references.
        /// </summary>
        /// <param name="typeName">The name of the type to create.</param>
        /// <param name="constructNewIfContainsReferences">Whether to construct a new instance if the type contains references.</param>
        /// <returns>A presumptive instance of the specified type.</returns>
        /// <remarks>
        /// A presumptive instance is a clone of a cached object of the given type, this assumes that the parameterless constructor of the type always returns the same object.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static object CreatePresumptiveInstance(string typeName, bool constructNewIfContainsReferences = false)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            return CreatePresumptiveInstance(TypeFactory.ResolveType(typeName, true), constructNewIfContainsReferences);
#pragma warning restore CS8604 // Possible null reference argument.
        }

        /// <summary>
        /// Creates a presumptive instance of the specified type in the specified assembly, optionally constructing a new instance if it contains references.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly containing the type.</param>
        /// <param name="typeName">The name of the type to create.</param>
        /// <param name="constructNewIfContainsReferences">Whether to construct a new instance if the type contains references.</param>
        /// <returns>A presumptive instance of the specified type.</returns>
        /// <remarks>
        /// A presumptive instance is a clone of a cached object of the given type, this assumes that the parameterless constructor of the type always returns the same object.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static object CreatePresumptiveInstance(string assemblyName, string typeName, bool constructNewIfContainsReferences = false)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            return CreatePresumptiveInstance(TypeFactory.ResolveType(assemblyName, typeName, true), constructNewIfContainsReferences);
#pragma warning restore CS8604 // Possible null reference argument.
        }

        /// <summary>
        /// Creates a clone of the specified object.
        /// </summary>
        /// <param name="obj">The object to clone.</param>
        /// <returns>The clone of the object.</returns>
        /// <remarks>
        /// This method uses the <see cref="System.Runtime.CompilerServices.Unsafe.CreateClone(object)"/> method to create a clone of the specified object. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static object CreateClone(object obj)
        {
            // If obj is null then we can't clone it, and if it's a value type then it doesn't need to be cloned.
            if (obj is null || obj.GetType().IsValueType)
#pragma warning disable CS8603 // Possible null reference return.
                return obj;
#pragma warning restore CS8603 // Possible null reference return.

            return Unsafe.CreateClone(obj);
        }
    }
}
