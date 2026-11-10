using Cassowary.Attributes;
using System.Runtime.InteropServices;

namespace Cassowary.Runtime.VM.EE
{
    /// <summary>
    ///     Native EEClassNativeLayoutInfo view (fieldmarshaler.h). The field descriptor
    ///     array is an inline trailing array at the aligned offset 16, not a pointer.
    ///     Linux x64 (UNIX_AMD64_ABI): passInRegisters at 1, marshalable at 2.
    ///     Windows x64: marshalable at 1.
    /// </summary>
    [UseAsPointer]
    [Intrinsic]
    [StructLayout(LayoutKind.Explicit)]
    public readonly unsafe struct EEClassNativeLayoutInfo
    {
        /// <summary>Base alignment requirement (m_alignmentRequirement).</summary>
        [FieldOffset(0)]
        public readonly byte AlignmentRequirement;

        /// <summary>
        ///     Whether the type is passed in registers (UNIX_AMD64_ABI only, offset 1).
        /// </summary>
        /// <exception cref="PlatformNotSupportedException">Windows runtime.</exception>
        public bool PassInRegisters
        {
            get
            {
                if (!RuntimeSupport.IsLinux)
                    throw new PlatformNotSupportedException("m_passInRegisters exists only under UNIX_AMD64_ABI.");

                fixed (EEClassNativeLayoutInfo* ptr = &this)
                {
                    return Unsafe.Read<bool>((byte*)ptr + 1);
                }
            }
        }

        /// <summary>
        ///     Whether the associated type can be marshaled (m_isMarshalable):
        ///     offset 2 on Linux (after m_passInRegisters), offset 1 on Windows.
        /// </summary>
        public bool IsMarshalable
        {
            get
            {
                fixed (EEClassNativeLayoutInfo* ptr = &this)
                {
                    return Unsafe.Read<bool>((byte*)ptr + (RuntimeSupport.IsLinux ? 2 : 1));
                }
            }
        }

        /// <summary>The size of the associated type (m_size, uint32 at offset 4).</summary>
        [FieldOffset(4)]
        public readonly uint Size;

        /// <summary>Number of NativeFieldDescriptors (m_numFields, uint32 at offset 8).</summary>
        [FieldOffset(8)]
        public readonly uint NumFields;

        /// <summary>
        ///     Pointer to the start of the inline NativeFieldDescriptor array trailing this
        ///     structure at the aligned offset 16. The array length is <see cref="NumFields"/>.
        /// </summary>
        public NativeFieldDescriptor* NativeFieldDescriptors
        {
            get
            {
                fixed (EEClassNativeLayoutInfo* ptr = &this)
                {
                    return (NativeFieldDescriptor*)((byte*)ptr + 16);
                }
            }
        }
    }
}
