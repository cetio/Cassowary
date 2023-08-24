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

namespace Cassowary.Runtime.VM
{
    // I don't like having this struct, especially since it doesn't actually exist in the Clr (it's a modified ArrayObject)
    // However, I'm too lazy to implement ArrayObject and StringObject, so it will stay.
    [Intrinsic]
    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct ArrayDesc
    {
        [FieldOffset(0)]
        public readonly int Length; // m_length

        [FieldOffset(4)]
        public readonly int Padding; // m_padding

        [FieldOffset(8)]
        internal readonly int ElemSize;

        [FieldOffset(12)]
        internal readonly int Rank;

        /// <summary>
        /// This is used in <see cref="MethodTable"/> to determine if an ArrayDesc is present.
        /// </summary>
        [FieldOffset(16)]
        internal byte Contained;

        public ArrayDesc(byte length, int size, int rank)
        {
            Length = length;
            Padding = 0;
            ElemSize = size;
            Rank = rank;
        }

        public override string ToString()
        {
            return $"ArrayDesc[{Length}]";
        }
    }
}
