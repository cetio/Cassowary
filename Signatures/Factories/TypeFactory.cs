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

using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Cassowary.Signatures.Factories
{
    public static class TypeFactory
    {
        private static Assembly[] _assemblies = AppDomain.CurrentDomain.GetAssemblies();
        private static AssemblyBuilder _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Factories"), AssemblyBuilderAccess.Run);
        private static ModuleBuilder _moduleBuilder = _assemblyBuilder.DefineDynamicModule("Cassowary");

        private static int _count = 0;
        private static Type[] _ctorTypes = new Type[]
        {
            typeof(object),
            typeof(nint)
        };

        /// <summary>
        /// Defines a delegate type dynamically.
        /// </summary>
        /// <param name="name">The name of the delegate type.</param>
        /// <param name="returnType">The return type of the delegate.</param>
        /// <param name="parameterTypes">The parameter types of the delegate.</param>
        /// <returns>The defined TypeBuilder for the delegate type.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        internal static TypeBuilder DefineDelegateType(string name, Type returnType, Type[] parameterTypes)
        {
            TypeBuilder typeBuilder = _moduleBuilder.DefineType(
                name + $"%{_count++}%",
                TypeAttributes.Public |
                TypeAttributes.Sealed |
                TypeAttributes.AutoClass,
                typeof(MulticastDelegate));
            typeBuilder.DefineConstructor(
                MethodAttributes.FamANDAssem |
                MethodAttributes.Family |
                MethodAttributes.HideBySig |
                MethodAttributes.RTSpecialName,
                CallingConventions.Standard,
                _ctorTypes)
                .SetImplementationFlags(MethodImplAttributes.CodeTypeMask);
            typeBuilder.DefineMethod("Invoke",
                MethodAttributes.FamANDAssem |
                MethodAttributes.Family |
                MethodAttributes.Virtual |
                MethodAttributes.HideBySig |
                MethodAttributes.VtableLayoutMask,
                returnType, parameterTypes)
                .SetImplementationFlags(MethodImplAttributes.CodeTypeMask);

            return typeBuilder;
        }

        /// <summary>
        /// Defines a type dynamically.
        /// </summary>
        /// <param name="name">The name of the type.</param>
        /// <param name="attr">The attributes of the type.</param>
        /// <param name="parent">The parent type of the type being defined.</param>
        /// <returns>The defined TypeBuilder for the type.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        internal static TypeBuilder DefineType(string name, TypeAttributes attr, Type parent)
        {
            return _moduleBuilder.DefineType(name + $"%{_count++}%", attr, parent);
        }

        /// <summary>
        /// Resolves a type by its name.
        /// </summary>
        /// <param name="typeName">The fully qualified name of the type.</param>
        /// <returns>The resolved Type, or null if not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        internal static Type? ResolveType(string typeName, bool throwOnNull = false)
        {
            Type? type;

            foreach (Assembly assembly in _assemblies)
            {
                if ((type = assembly.GetType(typeName)) != null)
                    return type;
            }

            if (throwOnNull)
                throw new TypeLoadException($"Cannot find Type of name {typeName}");

            return null;
        }
    }
}
