﻿//              FUCK OOP & FUCK INTERLACED LIBRARIES
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

namespace Cassowary.Runtime
{
    public static unsafe partial class Intrinsics
    {
        /// <summary>
        /// Converts a MethodInfo to a runtime MethodInfo object.
        /// </summary>
        /// <param name="methodInfo">The MethodInfo to convert.</param>
        /// <returns>The runtime MethodInfo object.</returns>
        public static object AsRuntimeMethodInfo(MethodInfo methodInfo)
        {
            // The type should always exist, if not, that's not my problem.
            return Cast(methodInfo, Type.GetType("System.Reflection.RuntimeMethodInfo")!);
        }

        /// <summary>
        /// Converts a ConstructorInfo to a runtime ConstructorInfo object.
        /// </summary>
        /// <param name="ctorInfo">The ConstructorInfo to convert.</param>
        /// <returns>The runtime ConstructorInfo object.</returns>
        public static object AsRuntimeConstructorInfo(ConstructorInfo ctorInfo)
        {
            // The type should always exist, if not, that's not my problem.
            return Cast(ctorInfo, Type.GetType("System.Reflection.RuntimeConstructorInfo")!);
        }

        /// <summary>
        /// Converts a RuntimeMethodHandle to a runtime RuntimeMethodHandleInternal object.
        /// </summary>
        /// <param name="methodHandle">The RuntimeMethodHandle to convert.</param>
        /// <returns>The runtime RuntimeMethodHandleInternal object.</returns>
        public static object AsRuntimeMethodHandleInternal(RuntimeMethodHandle methodHandle)
        {
            // The type should always exist, if not, that's not my problem.
            return CastNoChecks(methodHandle, Type.GetType("System.RuntimeMethodHandleInternal")!);
        }
    }
}
