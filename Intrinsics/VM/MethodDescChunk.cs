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

namespace Cassowary.Intrinsics.VM
{
    [Flags]
    public enum ChunkFlags : ushort
    {
        /// <summary>
        ///     This must equal METHOD_TOKEN_RANGE_MASK calculated higher in this file.
        ///     These are separate to allow the flags space available and used to be obvious here
        ///     and for the logic that splits the token to be algorithmically generated based on the #define
        /// </summary>
        TokenRangeMask = 0x0FFF,
        /// <summary>
        ///     Compact temporary entry points
        /// </summary>
        HasCompactEntryPoints = 0x4000
    }

    [ShouldUsePointerNotObject]
    [Intrinsic]
    [StructLayout(LayoutKind.Explicit)]
    public readonly unsafe struct MethodDescChunk
    {
        [FieldOffset(0)]
        public readonly MethodTable* DeclaringMethodTable; // m_methodTable

        [FieldOffset(8)]
        public readonly MethodDescChunk* NextChunk; // m_next

        [FieldOffset(16)]
        public readonly byte Size; // m_size

        [FieldOffset(17)]
        public readonly byte Count; // m_count

        [FieldOffset(18)]
        public readonly ChunkFlags ChunkFlags; // m_flagsAndTokenRange

        [FieldOffset(24)]
        public readonly MethodDesc FirstMethodDesc; // m_flagsAndTokenRange

        /// <summary>
        /// Gets a value determining whether or not the TokenRangeMask flag is present
        /// </summary>
        public bool HasTokenRangeMask
        {
            get
            {
                return ChunkFlags.HasFlag(ChunkFlags.TokenRangeMask);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the HasCompactEntryPoints flag is present
        /// </summary>
        public bool HasCompactEntryPoints
        {
            get
            {
                return ChunkFlags.HasFlag(ChunkFlags.HasCompactEntryPoints);
            }
        }
    }
}
