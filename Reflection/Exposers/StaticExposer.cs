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
using System.Runtime.CompilerServices;
using Unsafe = Cassowary.Intrinsics.Unsafe;

namespace Cassowary.Reflection.Exposers
{
    public sealed class StaticExposer<D>
    {
        public Type Type { get; private set; }
        public string MethodName { get; private set; }
        public object? Instance { get; private set; }
        public D Invoke { get; private set; }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public StaticExposer(Type type, string methodName, object? instance = null)
        {
            Type = type;
            MethodName = methodName;
            Instance = instance;

            MethodInfo methodInfo = type.GetMethod(
                methodName,
                BindingFlags.Static |
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic)
                ?? throw new ArgumentOutOfRangeException($"Cannot find method {methodName}");

            if (methodInfo.IsStatic)
            {
                Delegate @delegate = methodInfo.CreateDelegate(typeof(D));
                Invoke = Unsafe.As<Delegate, D>(ref @delegate);
            }
            else
            {
                Delegate @delegate = methodInfo.CreateDelegate(typeof(D), instance);
                Invoke = Unsafe.As<Delegate, D>(ref @delegate);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public StaticExposer(MethodInfo methodInfo, object? instance = null)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            Type = methodInfo.DeclaringType;
#pragma warning restore CS8601 // Possible null reference assignment.
            MethodName = methodInfo.Name;
            Instance = instance;

            if (methodInfo.IsStatic)
            {
                Delegate @delegate = methodInfo.CreateDelegate(typeof(D));
                Invoke = Unsafe.As<Delegate, D>(ref @delegate);
            }
            else
            {
                Delegate @delegate = methodInfo.CreateDelegate(typeof(D), instance);
                Invoke = Unsafe.As<Delegate, D>(ref @delegate);
            }
        }
    }
}
