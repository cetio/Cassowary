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
        public static object AsRuntimeMethodInfo(MethodInfo methodInfo)
        {
            // The type should always exist, if not, that's not my problem.
            return Cast(methodInfo, Type.GetType("System.Reflection.RuntimeMethodInfo")!);
        }

        public static object AsRuntimeConstructorInfo(ConstructorInfo ctorInfo)
        {
            // The type should always exist, if not, that's not my problem.
            return Cast(ctorInfo, Type.GetType("System.Reflection.RuntimeConstructorInfo")!);
        }

        public static object AsRuntimeMethodHandleInternal(RuntimeMethodHandle methodHandle)
        {
            // The type should always exist, if not, that's not my problem.
            return CastNoChecks(methodHandle, Type.GetType("System.RuntimeMethodHandleInternal")!);
        }

        public static Signature GetSignatureSafe(MethodInfo methodInfo)
        {
            return Signature.GetSignatureSafe(methodInfo);
        }

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
