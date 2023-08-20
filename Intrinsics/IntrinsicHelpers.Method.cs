using System.Reflection;
using System.Runtime.InteropServices;

namespace Cassowary.Intrinsics
{
    public static unsafe partial class IntrinsicHelpers
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
            return CastNoChecks(methodHandle, GetMethodTable(Type.GetType("System.RuntimeMethodHandleInternal")!));
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
