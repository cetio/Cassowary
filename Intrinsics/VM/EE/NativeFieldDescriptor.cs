//              FUCK OOP & FUCK INTERLACED LIBRARIES
//
//            DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//                   Version 2, December 2004
// 
// Copyright (C) 2004 Sam Hocevar<sam@hocevar.net>
//
// Everyone is permitted to copy and distribute verbatim or modified
// copies of this license document, and changing it is allowed as long
// as the name is changed.
//
//           DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//  TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION
//
//  0. You just DO WHAT THE FUCK YOU WANT TO.

using Cassowary.Attributes;
using System.Runtime.InteropServices;

namespace Cassowary.Intrinsics.VM.EE
{
    public enum NativeFieldCategory : short
    {
        FLOAT,
        NESTED,
        INTEGER,
        ILLEGAL
    }

    [ShouldUsePointerNotObject]
    [Intrinsic]
    [StructLayout(LayoutKind.Explicit)]
    public struct NativeFieldDescriptor
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct NestedTypeAndCount
        {
            [FieldOffset(0)] public MethodTable NestedType; // m_pNestedType

            [FieldOffset(4)] public uint NumElements; // m_numElements
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct NativeSizeAndAlignment
        {
            [FieldOffset(0)] public uint NativeSize; // nativeSize

            [FieldOffset(4)] public uint AlignmentRequirement; // alignmentRequirement
        }

        [FieldOffset(0)]
        public FieldDesc FieldDesc; // m_pFD

        [FieldOffset(4)]
        public NestedTypeAndCount TypeAndCount; // nestedTypeAndCount

        [FieldOffset(4)]
        public NativeSizeAndAlignment SizeAndAlignment; // nativeSizeAndAlignment

        [FieldOffset(8)]
        public uint Offset; // m_offset

        [FieldOffset(12)]
        public NativeFieldCategory Category; // m_category

        public MethodTable NestedType => TypeAndCount.NestedType;

        public uint NumElements => TypeAndCount.NumElements;

        public uint NativeSize => SizeAndAlignment.NativeSize;

        public uint AlignmentRequirement => SizeAndAlignment.AlignmentRequirement;

        public bool IsFloat => Category == NativeFieldCategory.FLOAT;

        public bool IsNested => Category == NativeFieldCategory.NESTED;

        public bool IsInteger => Category == NativeFieldCategory.INTEGER;

        public bool IsIllegal => Category == NativeFieldCategory.ILLEGAL;
    }
}
