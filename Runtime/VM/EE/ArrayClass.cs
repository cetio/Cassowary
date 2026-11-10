using Cassowary.Attributes;
using Cassowary.Runtime.Cor;
using System.Runtime.InteropServices;

namespace Cassowary.Runtime.VM.EE
{
    [UseAsPointer]
    [Intrinsic]
    [StructLayout(LayoutKind.Explicit)]
    public readonly unsafe struct ArrayClass
    {
        /// <summary>
        /// Multi-dimensional rank of the array.
        /// </summary>
        [FieldOffset(72)]
        public readonly byte Rank; // m_rank

        /// <summary>
        /// Cache of MethodTable's ElementMethodTable.
        /// </summary>
        [FieldOffset(76)]
        public readonly CorElementType CorElementType; // m_ElementType

        public bool IsPrimitive
        {
            get
            {
                return Intrinsics.CorIsPrimitiveType(CorElementType);
            }
        }
    }
}