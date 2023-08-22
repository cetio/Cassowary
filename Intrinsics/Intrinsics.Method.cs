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
using System.Runtime.InteropServices;

namespace Cassowary.Intrinsics
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

        /// <summary>
        /// Gets the signature of a method safely.
        /// </summary>
        /// <param name="methodInfo">The MethodInfo to get the signature for.</param>
        /// <returns>The method's signature.</returns>
        public static Signature GetSignatureSafe(MethodInfo methodInfo)
        {
            return Signature.GetSignatureSafe(methodInfo);
        }

        /// <summary>
        /// Checks if a method is a QCall (internal runtime call).
        /// </summary>
        /// <param name="methodInfo">The MethodInfo to check.</param>
        /// <returns>True if the method is a QCall, otherwise false.</returns>
        public static bool IsQCall(MethodInfo methodInfo)
        {
            return (methodInfo.GetCustomAttribute<LibraryImportAttribute>() != null &&
                methodInfo.GetCustomAttribute<LibraryImportAttribute>()!.EntryPoint == "QCall") ||
                (methodInfo.GetCustomAttribute<DllImportAttribute>() != null &&
                methodInfo.GetCustomAttribute<DllImportAttribute>()!.EntryPoint == "QCall") ||
                methodInfo.MethodImplementationFlags.HasFlag(MethodImplAttributes.InternalCall);
        }
    }
}
