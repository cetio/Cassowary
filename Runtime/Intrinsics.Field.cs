using System.Reflection;

namespace Cassowary.Runtime
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
