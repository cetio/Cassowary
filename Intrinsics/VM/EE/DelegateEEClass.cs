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
using JetBrains.Annotations;

using System.Runtime.InteropServices;

namespace Cassowary.Intrinsics.VM.EE
{
    [ShouldUsePointerNotObject]
    [Intrinsic]
    [StructLayout(LayoutKind.Explicit)]
    public readonly unsafe struct DelegateEEClass
    {
        [FieldOffset(72)]
        public readonly Stub* StaticCallStub; // m_pStaticCallStub

        [FieldOffset(80)]
        public readonly Stub* InstRetBuffCallStub; // m_pInstRetBuffCallStub

        [FieldOffset(88)]
        public readonly MethodDesc* InvokeMethod; // m_pInvokeMethod

        [FieldOffset(96)]
        public readonly Stub* MultiCastInvokeStub; // m_pMultiCastInvokeStub

        [FieldOffset(104)]
        public readonly Stub* WrapperDelegateInvokeStub; // m_pWrapperDelegateInvokeStub

        [FieldOffset(112)]
        public readonly UMThunkMarshInfo* UMThunkMarshInfo; // m_pUMThunkMarshInfo

        [FieldOffset(120)]
        public readonly MethodDesc* BeginInvokeMethod; // m_pBeginInvokeMethod

        [FieldOffset(128)]
        public readonly MethodDesc* EndInvokeMethod; // m_pEndInvokeMethod

        [CanBeNull]
        [FieldOffset(136)]
        public readonly byte* MarshalStub; // m_pMarshalStub
    }
}