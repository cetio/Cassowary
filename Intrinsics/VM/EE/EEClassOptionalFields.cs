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
    [StructLayout(LayoutKind.Explicit)]
    public readonly unsafe struct EEClassOptionalFields
    {
        // GENERICS RELATED FIELDS

        // If IsSharedByGenericInstantiations(), layout of handle dictionary for generic type
        // (the last dictionary pointed to from PerInstInfo). Otherwise NULL.
        [FieldOffset(0)]
        public readonly byte* DictionaryLayout; //m_pDictLayout

        // Variance info for each type parameter (gpNonVariant, gpCovariant, or gpContravariant)
        // If NULL, this type has no type parameters that are co/contravariant
        [FieldOffset(8)]
        public readonly byte* VarianceInfo; //m_pVarianceInfo

        [FieldOffset(16)]
        public readonly SparseVTableMap* SparseVTableMap; //m_pSparseVTableMap

        [FieldOffset(24)]
        public readonly RuntimeTypeHandle CoClass; //m_pCoClassForIntf

        // COM RELATED FIELDS

        // Note: You may need to define the SparseVTableMap and TypeHandle classes
        // if they're not already defined in your project.
        // public SparseVTableMap m_pSparseVTableMap;
        // public TypeHandle m_pCoClassForIntf;
        // public ClassFactoryBase m_pClassFactory;

        [FieldOffset(12)]
        public readonly uint ModuleDynamicID; //m_cbModuleDynamicID

        [FieldOffset(16)]
        public readonly byte RequiredFieldAlignment; //m_requiredFieldAlignment
    }
}