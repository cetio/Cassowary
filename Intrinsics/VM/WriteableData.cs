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
    [Flags]
    public enum WriteableFlags : uint
    {
        Unrestored = 0x00000004,
        HasApproxParent = 0x00000010,
        UnrestoredTypeKey = 0x00000020,
        IsNotFullyLoaded = 0x00000040,
        DependenciesLoaded = 0x00000080,     // class and all dependencies loaded up to CLASS_LOADED_BUT_NOT_VERIFIED

        CanCompareBitsOrUseFastGetHashCode = 0x00000200,     // Is any field type or sub field type overrode Equals or GetHashCode
        HasCheckedCanCompareBitsOrUseFastGetHashCode = 0x00000400,  // Whether we have checked the overridden Equals or GetHashCode

        ParentMethodTablePointerValid = 0x40000000,
        HasInjectedInterfaceDuplicates = 0x80000000,
    }

    [Intrinsic]
    [StructLayout(LayoutKind.Explicit)]
    public readonly unsafe struct WriteableData
    {
        /// <summary>
        /// Flags pertaining to the parent structure (MethodTable.)
        /// </summary>
        [FieldOffset(0)]
        public readonly WriteableFlags WriteableFlags; // m_dwFlags

        //[FieldOffset(4)]
        //public readonly RuntimeTypeHandle m_hExposedClassObject;

        public bool IsUnrestored
        {
            get
            {
                return WriteableFlags.HasFlag(WriteableFlags.Unrestored);
            }
        }

        public bool HasApproxParent
        {
            get
            {
                return WriteableFlags.HasFlag(WriteableFlags.HasApproxParent);
            }
        }

        public bool HasUnrestoredTypeKey
        {
            get
            {
                return WriteableFlags.HasFlag(WriteableFlags.UnrestoredTypeKey);
            }
        }

        public bool IsNotFullyLoaded
        {
            get
            {
                return WriteableFlags.HasFlag(WriteableFlags.IsNotFullyLoaded);
            }
        }

        public bool DependenciesLoaded
        {
            get
            {
                return WriteableFlags.HasFlag(WriteableFlags.DependenciesLoaded);
            }
        }

        public bool CanCompareBitsOrUseFastGetHashCode
        {
            get
            {
                return WriteableFlags.HasFlag(WriteableFlags.CanCompareBitsOrUseFastGetHashCode);
            }
        }

        public bool HasCheckedCanCompareBitsOrUseFastGetHashCode
        {
            get
            {
                return WriteableFlags.HasFlag(WriteableFlags.HasCheckedCanCompareBitsOrUseFastGetHashCode);
            }
        }

        public bool ParentMethodTablePointerValid
        {
            get
            {
                return WriteableFlags.HasFlag(WriteableFlags.ParentMethodTablePointerValid);
            }
        }

        public bool HasInjectedInterfaceDuplicates
        {
            get
            {
                return WriteableFlags.HasFlag(WriteableFlags.HasInjectedInterfaceDuplicates);
            }
        }
    }
}
