using Cassowary.Attributes;
using System.Runtime.InteropServices;

namespace Cassowary.Runtime.VM.EE
{
    /// <summary>NativeFieldCategory from fieldmarshaler.h (short enum).</summary>
    public enum NativeFieldCategory : short
    {
        FLOAT,
        NESTED,
        INTEGER,
        ILLEGAL
    }

    /// <summary>
    ///     Native NativeFieldDescriptor view (32 bytes on x64):
    ///     FieldDesc* at 0, 16-byte union at 8 (nested type at 8 / element count at 16, or
    ///     native size at 8 / alignment at 12), offset at 24, category at 28.
    /// </summary>
    [UseAsPointer]
    [Intrinsic]
    [StructLayout(LayoutKind.Explicit, Size = 32)]
    public readonly unsafe struct NativeFieldDescriptor
    {
        /// <summary>Pointer to the managed FieldDesc this native field marshals (m_pFD).</summary>
        [FieldOffset(0)]
        public readonly FieldDesc* FieldDesc;

        /// <summary>Nested field: MethodTable of the nested type (union member, offset 8).</summary>
        [FieldOffset(8)]
        public readonly MethodTable* NestedType;

        /// <summary>Nested field: element count (union member, offset 16, ULONG=4 bytes).</summary>
        [FieldOffset(16)]
        public readonly uint NumElements;

        /// <summary>Simple field: native size in bytes (union member, offset 8).</summary>
        [FieldOffset(8)]
        public readonly uint NativeSize;

        /// <summary>Simple field: alignment requirement (union member, offset 12).</summary>
        [FieldOffset(12)]
        public readonly uint AlignmentRequirement;

        /// <summary>Native field offset (m_offset).</summary>
        [FieldOffset(24)]
        public readonly uint Offset;

        /// <summary>Field category (m_category, short enum at offset 28).</summary>
        [FieldOffset(28)]
        public readonly NativeFieldCategory Category;

        /// <summary>Whether the field is of float category.</summary>
        public bool IsFloat => Category == NativeFieldCategory.FLOAT;

        /// <summary>Whether the field is nested.</summary>
        public bool IsNested => Category == NativeFieldCategory.NESTED;

        /// <summary>Whether the field is of integer category.</summary>
        public bool IsInteger => Category == NativeFieldCategory.INTEGER;

        /// <summary>Whether the field is illegal to marshal.</summary>
        public bool IsIllegal => Category == NativeFieldCategory.ILLEGAL;
    }
}
