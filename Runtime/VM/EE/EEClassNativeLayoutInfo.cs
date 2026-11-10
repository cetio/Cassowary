using Cassowary.Attributes;
using System.Runtime.InteropServices;

namespace Cassowary.Runtime.VM.EE
{
    [Intrinsic]
    [StructLayout(LayoutKind.Explicit)]
    public readonly unsafe struct EEClassNativeLayoutInfo
    {
        /// <summary>
        /// Base alignment requirement.
        /// </summary>
        [FieldOffset(0)]
        public readonly byte AlignmentRequirement; // m_alignmentRequirement

        /// <summary>
        /// Determines if the associated type is able to be marshalled (unmanaged.)
        /// </summary>
        [FieldOffset(1)]
        public readonly bool IsMarshalable; // m_isMarshalable

        /// <summary>
        /// The size of the associated type.
        /// </summary>
        [FieldOffset(2)]
        public readonly ushort Size; // m_size

        /// <summary>
        /// Number of NativeFieldDescriptors.
        /// </summary>
        [FieldOffset(4)]
        public readonly uint NumFields; // m_numFields

        /// <summary>
        /// Points to the start of a NativeFieldDescriptor array, length == m_numFields.
        /// </summary>
        [FieldOffset(12)]
        public readonly NativeFieldDescriptor* NativeFieldDescriptors; // m_nativeFieldDescriptors
    }
}
