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
    [ShouldUsePointerNotObject]
    [Intrinsic]
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct SparseVTableMap
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Entry
        {
            /// <summary>
            /// Starting MT slot number
            /// </summary>
            public ushort Start;

            /// <summary>
            /// # of consecutive slots that map linearly
            /// </summary>
            public ushort Span;

            /// <summary>
            /// Starting VT slot number
            /// </summary>
            public ushort MapTo;
        }

        /// <summary>
        /// Array of Entry structures.
        /// </summary>
        public readonly Entry* EntryMapList; // m_MapList

        /// <summary>
        /// Number of entries in the array.
        /// </summary>
        public readonly ushort NumMapEntries; // m_MapEntries

        /// <summary>
        /// Number of entries allocated.
        /// </summary>
        public readonly ushort NumAllocated; // m_Allocated

        /// <summary>
        /// Index of last entry used in successful lookup.
        /// </summary>
        public readonly ushort LastUsedIndex; // m_LastUsed

        /// <summary>
        /// Current VT slot number, used during list build
        /// </summary>
        public readonly ushort VTSlot; // m_VTSlot

        /// <summary>
        /// Current MT slot number, used during list build
        /// </summary>
        public readonly ushort MTSlot; // m_MTSlot
    }
}
