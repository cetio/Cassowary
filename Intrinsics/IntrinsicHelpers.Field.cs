using System.Reflection;

namespace Cassowary.Intrinsics
{
    public static unsafe partial class IntrinsicHelpers
    {
        public static object AsRuntimeFieldInfo(FieldInfo fieldInfo)
        {
            // The type should always exist, if not, that's not my problem.
            return Cast(fieldInfo, Type.GetType("System.Reflection.RtFieldInfo")!);
        }

        public static object AsRuntimeFieldHandleInternal(RuntimeFieldHandle fieldHandle)
        {
            // The type should always exist, if not, that's not my problem.
            return CastNoChecks(fieldHandle, GetMethodTable(Type.GetType("System.RuntimeFieldHandleInternal")!));
        }
    }
}
