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

namespace Cassowary.Runtime.VM.EE
{
    public enum NativeFieldCategory : short
    {
        FLOAT,
        NESTED,
        INTEGER,
        ILLEGAL
    }

    [UseAsPointer]
    [Intrinsic]
    [StructLayout(LayoutKind.Explicit)]
    public readonly unsafe struct NativeFieldDescriptor
    {
        /// <summary>
        /// Represents the type and count of a nested field.
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        public unsafe struct NestedTypeAndCount
        {
            [FieldOffset(0)] public MethodTable* NestedType; // m_pNestedType

            [FieldOffset(8)] public uint NumElements; // m_numElements
        }

        /// <summary>
        /// Represents the size and alignment of a native field.
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        public struct NativeSizeAndAlignment
        {
            [FieldOffset(0)] public uint NativeSize; // nativeSize

            [FieldOffset(4)] public uint AlignmentRequirement; // alignmentRequirement
        }

        [FieldOffset(0)]
        public readonly FieldDesc FieldDesc; // m_pFD

        [FieldOffset(4)]
        public readonly NestedTypeAndCount TypeAndCount; // nestedTypeAndCount

        [FieldOffset(4)]
        public readonly NativeSizeAndAlignment SizeAndAlignment; // nativeSizeAndAlignment

        [FieldOffset(8)]
        public readonly uint Offset; // m_offset

        [FieldOffset(12)]
        public readonly NativeFieldCategory Category; // m_category

        /// <summary>
        /// Gets the nested MethodTable of the field.
        /// </summary>
        public MethodTable* NestedType
        {
            get
            {
                return TypeAndCount.NestedType;
            }
        }

        /// <summary>
        /// Gets the number of elements in the field.
        /// </summary>
        public uint NumElements
        {
            get
            {
                return TypeAndCount.NumElements;
            }
        }

        /// <summary>
        /// Gets the native size of the field.
        /// </summary>
        public uint NativeSize
        {
            get
            {
                return SizeAndAlignment.NativeSize;
            }
        }

        /// <summary>
        /// Gets the alignment requirement of the field.
        /// </summary>
        public uint AlignmentRequirement
        {
            get
            {
                return SizeAndAlignment.AlignmentRequirement;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the field is of float category.
        /// </summary>
        public bool IsFloat
        {
            get
            {
                return Category == NativeFieldCategory.FLOAT;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the field is nested.
        /// </summary>
        public bool IsNested
        {
            get
            {
                return Category == NativeFieldCategory.NESTED;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the field is of integer category.
        /// </summary>
        public bool IsInteger
        {
            get
            {
                return Category == NativeFieldCategory.INTEGER;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the field is illegal.
        /// </summary>
        public bool IsIllegal
        {
            get
            {
                return Category == NativeFieldCategory.ILLEGAL;
            }
        }
    }
}
