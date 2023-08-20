using Cassowary.Intrinsics.VM;
using Cassowary.Intrinsics.VM.Cor;
using System.Runtime.CompilerServices;

namespace Cassowary.Intrinsics
{
    public static unsafe partial class IntrinsicHelpers
    {
        /// <summary>
        /// Gets the MethodTable of the provided type.
        /// </summary>
        /// <param name="obj">The object to clone.</param>
        /// <returns>A pointer to the MethodTable of the provided type.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodTable* GetMethodTable(Type type)
        {
            return MethodTable.FromType(type);
        }

        public static object AsRuntimeType(Type type)
        {
            // The type should always exist, if not, that's not my problem.
            return Cast(type, Type.GetType("System.RuntimeType")!);
        }

        public static bool CorIsPrimitiveType(CorElementType elementtype)
        {
            return elementtype < CorElementType.ELEMENT_TYPE_PTR || elementtype == CorElementType.ELEMENT_TYPE_I || elementtype == CorElementType.ELEMENT_TYPE_U;
        }
    }
}
