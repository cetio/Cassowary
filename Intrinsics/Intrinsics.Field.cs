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

namespace Cassowary.Intrinsics
{
    public static unsafe partial class Intrinsics
    {
        /// <summary>
        /// Converts a FieldInfo object to a runtime FieldInfo.
        /// </summary>
        /// <param name="fieldInfo">The FieldInfo to convert.</param>
        /// <returns>The runtime FieldInfo object.</returns>
        public static object AsRuntimeFieldInfo(FieldInfo fieldInfo)
        {
            // The type should always exist, if not, that's not my problem.
            return Cast(fieldInfo, Type.GetType("System.Reflection.RtFieldInfo")!);
        }

        /// <summary>
        /// Converts a RuntimeFieldHandle to a runtime FieldHandle.
        /// </summary>
        /// <param name="fieldHandle">The RuntimeFieldHandle to convert.</param>
        /// <returns>The runtime FieldHandle object.</returns>
        public static object AsRuntimeFieldHandleInternal(RuntimeFieldHandle fieldHandle)
        {
            // The type should always exist, if not, that's not my problem.
            return CastNoChecks(fieldHandle, Type.GetType("System.RuntimeFieldHandleInternal")!);
        }
    }
}
