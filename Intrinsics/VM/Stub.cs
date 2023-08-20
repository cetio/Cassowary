﻿//              FUCK OOP & FUCK INTERLACED LIBRARIES
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
    public enum StubCodeFlags : uint
    {
        MULTICAST_DELEGATE_BIT = 0x80000000,
        EXTERNAL_ENTRY_BIT = 0x40000000,
        LOADER_HEAP_BIT = 0x20000000,
        INSTANTIATING_STUB_BIT = 0x10000000,
        UNWIND_INFO_BIT = 0x08000000,
        THUNK_BIT = 0x04000000,

        CODEBYTES_MASK = THUNK_BIT - 1,
        MAX_CODEBYTES = CODEBYTES_MASK + 1,
    };

    [ShouldUsePointerNotObject]
    [Intrinsic]
    [StructLayout(LayoutKind.Explicit)]
    public readonly unsafe struct Stub
    {
        [FieldOffset(0)]
        public readonly uint RefCount; // m_refcount

        [FieldOffset(4)]
        public readonly StubCodeFlags CodeFlags; // m_numCodeBytesAndFlags

        [FieldOffset(4)]
        public readonly uint NumCodeBytes; // m_numCodeBytesAndFlags

        [FieldOffset(6)]
        public readonly ushort PatchOffset; // PatchOffset

        [FieldOffset(6)]
        public readonly MethodDesc* InstantiatedMethod; // InstantiatedMethod

        public bool IsMulticastDelegate => CodeFlags.HasFlag(StubCodeFlags.MULTICAST_DELEGATE_BIT);

        public bool HasExternalEntryPoint => CodeFlags.HasFlag(StubCodeFlags.EXTERNAL_ENTRY_BIT);

        public bool IsLoaderHeap => CodeFlags.HasFlag(StubCodeFlags.LOADER_HEAP_BIT);

        public bool IsIntantiatingStub => CodeFlags.HasFlag(StubCodeFlags.INSTANTIATING_STUB_BIT);

        public bool HasUnwindInfo => CodeFlags.HasFlag(StubCodeFlags.UNWIND_INFO_BIT);

        public bool HasThunk => CodeFlags.HasFlag(StubCodeFlags.THUNK_BIT);
    }
}
