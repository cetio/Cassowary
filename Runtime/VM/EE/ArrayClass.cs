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