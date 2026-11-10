using Cassowary.Runtime.Cor;
using Cassowary.Runtime.VM;
using System.Runtime.CompilerServices;

namespace Cassowary.Runtime
{
    public static unsafe partial class Intrinsics
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

        /// <summary>
        /// Converts a <see cref="Type"/> to a runtime type object.
        /// </summary>
        /// <param name="type">The type to convert.</param>
        /// <returns>A runtime type object representing the provided type.</returns>
        public static object AsRuntimeType(Type type)
        {
            // The type should always exist, if not, that's not my problem.
            return Cast(type, Type.GetType("System.RuntimeType")!);
        }

        /// <summary>
        /// Checks if a <see cref="CorElementType"/> is a primitive type.
        /// </summary>
        /// <param name="elementType">The element type to check.</param>
        /// <returns><c>true</c> if the element type is a primitive type; otherwise, <c>false</c>.</returns>
        internal static bool CorIsPrimitiveType(CorElementType elementtype)
        {
            return elementtype < CorElementType.ELEMENT_TYPE_PTR || elementtype == CorElementType.ELEMENT_TYPE_I || elementtype == CorElementType.ELEMENT_TYPE_U;
        }
    }
}
