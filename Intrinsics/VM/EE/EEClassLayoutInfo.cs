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
    [Flags]
    public enum LayoutFlags : byte
    {
        /// <summary>
        ///     TRUE if the GC layout of the class is bit-for-bit identical
        ///     to its unmanaged counterpart (i.e. no internal reference fields,
        ///     no ansi-unicode char conversions required, etc.) Used to
        ///     optimize marshaling.
        /// </summary>
        Blittable = 0x01,
        /// <summary>
        ///     Is this type also sequential in managed memory?
        /// </summary>
        ManagedSequential = 0x02,
        /// <summary>
        ///     When a sequential/explicit type has no fields, it is conceptually
        ///     zero-sized, but actually is 1 byte in length. This holds onto this
        ///     fact and allows us to revert the 1 byte of padding when another
        ///     explicit type inherits from this type.
        /// </summary>
        ZeroSized = 0x04,
        /// <summary>
        ///     The size of the struct is explicitly specified in the meta-data.
        /// </summary>
        HasExplicitSize = 0x08,

        /// <summary>
        ///     Whether a native struct is passed in registers.
        /// </summary>
        NativePassInRegisters = 0x10,
        R4HFA = 0x10,
        R8HFA = 0x20
    }

    [Intrinsic]
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct EEClassLayoutInfo
    {
        [FieldOffset(0)]
        public int ManagedSize;

        // 1,2,4 or 8: this is equal to the largest of the alignment requirements
        // of each of the EEClass's members. If the NStruct extends another NStruct,
        // the base NStruct is treated as the first member for the purpose of
        // this calculation.

        // Post V1.0 addition: This is the equivalent of m_LargestAlignmentRequirementOfAllMember
        // for the managed layout.

        [FieldOffset(4)]
        public readonly byte ManagedLargestAlignmentRequirementOfAllMembers;

        /// <summary>
        /// Managed layout flags
        /// </summary>
        [FieldOffset(5)]
        public readonly LayoutFlags LayoutFlags;

        /// <summary>
        /// Packing size in bytes (1, 2, 4, 8 etc.)
        /// </summary>
        [FieldOffset(9)]
        public readonly byte PackingSize;

        public bool IsBlittable
        {
            get
            {
                return LayoutFlags.HasFlag(LayoutFlags.Blittable);
            }
        }

        public bool IsManagedSequential
        {
            get
            {
                return LayoutFlags.HasFlag(LayoutFlags.ManagedSequential);
            }
        }

        public bool IsZeroSized
        {
            get
            {
                return LayoutFlags.HasFlag(LayoutFlags.ZeroSized);
            }
        }

        public bool HasExplicitSize
        {
            get
            {
                return LayoutFlags.HasFlag(LayoutFlags.HasExplicitSize);
            }
        }

        public bool IsNativePassInRegisters
        {
            get
            {
                return LayoutFlags.HasFlag(LayoutFlags.NativePassInRegisters);
            }
        }

        public bool IsR4HFA
        {
            get
            {
                return LayoutFlags.HasFlag(LayoutFlags.R4HFA);
            }
        }

        public bool IsR8HFA
        {
            get
            {
                return LayoutFlags.HasFlag(LayoutFlags.R8HFA);
            }
        }
    }
}