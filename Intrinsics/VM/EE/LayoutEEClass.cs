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
    public readonly unsafe struct LayoutEEClass
    {
        [FieldOffset(72)]
        public readonly EEClassLayoutInfo LayoutInfo; //m_LayoutInfo

        [CanBeNull]
        [FieldOffset(80)]
        public readonly EEClassNativeLayoutInfo* NativeLayoutInfo; //m_nativeLayoutInfo

        public bool HasNativeLayoutInfo
        {
            get
            {
                try
                {
                    if ((nint)NativeLayoutInfo == 0)
                        return false;

                    // This is just intended to cause a object reference error (if the pointer doesn't point to EEClassOptionalFields)
                    return (object?)*NativeLayoutInfo != null;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
