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
    public enum StorageKind
    {
        Unknown,
        Explicit,
        Synthetic
    };

    [Intrinsic]
    [StructLayout(LayoutKind.Explicit)]
    public readonly unsafe struct NativeCodeVersion
    {
        [FieldOffset(0)]
        public readonly StorageKind StorageKind; // m_storageKind
    }
}
