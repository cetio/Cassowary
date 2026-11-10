using Cassowary.Attributes;
using Cassowary.Runtime.Cor;
using System.Runtime.InteropServices;

namespace Cassowary.Runtime.VM.EE
{
    /// <summary>
    ///     ArrayClass view: derived EEClass fields begin immediately after the platform
    ///     dependent base (Windows 72 / Linux 56 bytes).
    /// </summary>
    [UseAsPointer]
    [Intrinsic]
    [StructLayout(LayoutKind.Explicit)]
    public readonly unsafe struct ArrayClass
    {
        /// <summary>Multi-dimensional rank of the array (m_rank, byte).</summary>
        public byte Rank => Unsafe.Read<byte>((byte*)Unsafe.AsPointer(ref Unsafe.AsRef(in this)) + EEClass.BaseSize);

        /// <summary>Cached element CorElementType (m_ElementType, 4-byte enum).</summary>
        public CorElementType CorElementType => (CorElementType)Unsafe.Read<int>((byte*)Unsafe.AsPointer(ref Unsafe.AsRef(in this)) + EEClass.BaseSize + 4);

        /// <summary>Whether the cached element type is a primitive type.</summary>
        public bool IsPrimitive
        {
            get
            {
                return Intrinsics.CorIsPrimitiveType(CorElementType);
            }
        }
    }
}
