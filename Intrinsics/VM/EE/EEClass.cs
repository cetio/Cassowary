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
using Cassowary.Intrinsics.VM.Cor;
using JetBrains.Annotations;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;

namespace Cassowary.Intrinsics.VM.EE
{
    [Flags]
    public enum VMFlags : uint
    {
        LayoutDependsOnOtherModules = 0x00000001,
        Delegate = 0x00000002,

        /// <summary>
        ///     Value type Statics in this class will be pinned
        /// </summary>
        FixedAddressVtStatics = 0x00000020,
        HasLayout = 0x00000040,
        IsNested = 0x00000080,
        IsEquivalentType = 0x00000200,

        //   OVERLAYED is used to detect whether Equals can safely optimize to a bit-compare across the structure.
        HasOverlayedFields = 0x00000400,

        // Set this if this class or its parent have instance fields which
        // must be explicitly inited in a constructor (e.g. pointers of any
        // kind, gc or native).
        //
        // Currently this is used by the verifier when verifying value classes
        // - it's ok to use uninitialized value classes if there are no
        // pointer fields in them.
        HasFieldsWhichMustBeInited = 0x00000800,

        UnsafeValueType = 0x00001000,

        /// <summary>
        ///     <see cref="BestFitMapping" /> and <see cref="ThrowOnUnmappableChar" /> are valid only if this is set
        /// </summary>
        BestFitMappingInited = 0x00002000,
        BestFitMapping = 0x00004000, // BestFitMappingAttribute.Value
        ThrowOnUnmappableChar = 0x00008000, // BestFitMappingAttribute.ThrowOnUnmappableChar

        // unused                              = 0x00010000,
        NoGuid = 0x00020000,
        HasNonPublicFields = 0x00040000,

        // unused                              = 0x00080000,
        ContainsStackPtr = 0x00100000,

        /// <summary>
        ///     Would like to have 8-byte alignment
        /// </summary>
        PreferAlign8 = 0x00200000,
        // unused                              = 0x00400000,

        SparseForCominterop = 0x00800000,

        // interfaces may have a coclass attribute
        HasCoClassAttrib = 0x01000000,
        ComEventItfMask = 0x02000000, // class is a special COM event interface
        ProjectedFromWinRT = 0x04000000,
        ExportedToWinRT = 0x08000000,

        // This one indicates that the fields of the valuetype are
        // not tightly packed and is used to check whether we can
        // do bit-equality on value types to implement ValueType::Equals.
        // It is not valid for classes, and only matters if ContainsPointer
        // is false.
        NotTightlyPacked = 0x10000000,

        // True if methoddesc on this class have any real (non-interface) methodimpls
        ContainsMethodImpls = 0x20000000,
        MarshalingTypeMask = 0xc0000000,
        MarshalingTypeInhibit = 0x40000000,
        MarshalingTypeFreeThreaded = 0x80000000,
        MarshalingTypeStandard = 0xc0000000
    }

    [ShouldUsePointerNotObject]
    [Intrinsic]
    [StructLayout(LayoutKind.Explicit)]
    public readonly unsafe struct EEClass
    {
        /// <summary>
        /// Contains the GuidInfo if the EEClass is associated with an Interface.
        /// </summary>
        [CanBeNull]
        [FieldOffset(0)]
        public readonly GuidInfo* GuidInfo; // m_pGuidInfo

        /// <summary>
        /// Contains Optional Fields if specified by the associated type.
        /// </summary>
        [CanBeNull]
        [FieldOffset(8)]
        public readonly EEClassOptionalFields* OptionalFields; // m_rpOptionalFields

        [FieldOffset(16)]
        public readonly MethodTable* MethodTable; // m_pMethodTable

        [FieldOffset(24)]
        public readonly FieldDesc* FieldDescList; // m_pFieldDescList

        [FieldOffset(32)]
        public readonly MethodDescChunk* MethodDescChunk; // m_pChunks

        [FieldOffset(40)]
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
        public readonly ObjectHandle* DelegateObjectHandle; // m_ohDelegate
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

        [FieldOffset(40)]
        public readonly CorInterfaceType ComInterfaceType; // m_ComInterfaceType

        /// <summary>
        /// Contains the CCWTemplate if the EEClass is associated with a COM Visible type.
        /// </summary>
        [CanBeNull]
        [FieldOffset(48)]
        public readonly CCWTemplate* CCWTemplate; // m_pccwTemplate

        [FieldOffset(56)]
        public readonly TypeAttributes ClassAttributes; // m_dwAttrClass

        [FieldOffset(60)]
        public readonly VMFlags VMFlags; // m_VMFlags

        [FieldOffset(64)]
        private readonly CorElementType CorElementType; // m_NormType

        [FieldOffset(65)]
        public readonly bool FieldsArePacked; // m_fFieldsArePacked

        [FieldOffset(66)]
        public readonly byte FixedEEClassFieldsSize; // m_cbFixedEEClassFields

        [FieldOffset(67)]
        public readonly byte BaseSizePadding; // m_cbBaseSizePadding

        /// <summary>
        /// Gets the rank of the array.
        /// </summary>
        /// <remarks>
        /// If the type is not an array or is a single dimensional array, the rank will be 0.
        /// </remarks>
        public int Rank
        {
            get
            {
                if (HasArrayClass)
                {
                    if (AsArrayClass()->Rank == 1)
                    {
                        return 0;
                    }
                    else
                    {
                        return AsArrayClass()->Rank - 1;
                    }
                }
                else if (HasArrayDesc)
                {
                    if (MethodTable->ArrayDesc.Rank == 1)
                    {
                        return 0;
                    }
                    else
                    {
                        return MethodTable->ArrayDesc.Rank - 1;
                    }
                }

                return 0;
            }
        }

        /// <summary>
        /// Gets the managed size of the array.
        /// </summary>
        /// <remarks>
        /// The managed size is the size of the array in memory, including the space for the element type and any padding.
        /// </remarks>
        public int ManagedSize
        {
            get
            {
                if (HasArrayDesc)
                {
                    int arrayDescSize = LayoutEEClass->LayoutInfo.ManagedSize;
                    int componentSize = MethodTable->ComponentSize;
                    int arrayLength = MethodTable->ArrayDesc.Length;
                    int size = arrayDescSize + componentSize * arrayLength - Rank;
                    return size;
                }
                else if (HasArrayClass)
                {
                    int arrayClassSize = LayoutEEClass->LayoutInfo.ManagedSize;
                    int size = arrayClassSize + LayoutEEClass->LayoutInfo.ManagedSize * Rank - Rank;
                    return size;
                }

                return LayoutEEClass->LayoutInfo.ManagedSize;
            }
        }

        /// <summary>
        /// Gets the layout information for the EEClass.
        /// </summary>
        public LayoutEEClass* LayoutEEClass
        {
            get
            {
                fixed (EEClass* ptr = &this)
                {
                    return (LayoutEEClass*)ptr;
                }
            }
        }

        /// <summary>
        /// EEClasses only contain a GuidInfo if it is associated with an Interface.
        /// </summary>
        public bool HasGuidInfo => NoGuid && MethodTable->IsInterfaceWithGuidInfo;

        /// <summary>
        /// EEClasses only contain Optional Fields if specified so by the associated type.
        /// </summary>
        public bool HasOptionalFields
        {
            get
            {
                try
                {
                    if ((nint)OptionalFields == 0)
                        return false;

                    // This is just intended to cause a object reference error (if the pointer doesn't point to EEClassOptionalFields)
                    return (object?)*OptionalFields != null;
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool HasArrayDesc => MethodTable->HasArrayDesc;

        public bool HasArrayClass => CorElementType == CorElementType.ELEMENT_TYPE_ARRAY || CorElementType == CorElementType.ELEMENT_TYPE_SZARRAY;

        public bool IsPrimitive => IntrinsicHelpers.CorIsPrimitiveType(CorElementType);

        /// <summary>
        /// Checks if the class layout depends on other modules.
        /// </summary>
        public bool LayoutDependsOnOtherModules => (VMFlags & VMFlags.LayoutDependsOnOtherModules) != 0;

        /// <summary>
        /// Checks if the class is a delegate.
        /// </summary>
        public bool IsDelegate => (VMFlags & VMFlags.Delegate) != 0;

        /// <summary>
        /// Checks if value type statics in this class will be pinned.
        /// </summary>
        public bool FixedAddressVtStatics => (VMFlags & VMFlags.FixedAddressVtStatics) != 0;

        /// <summary>
        /// Checks if the class has layout information.
        /// </summary>
        public bool HasLayout => (VMFlags & VMFlags.HasLayout) != 0;

        /// <summary>
        /// Checks if the class is nested.
        /// </summary>
        public bool IsNested => (VMFlags & VMFlags.IsNested) != 0;

        /// <summary>
        /// Checks if the class is an equivalent type.
        /// </summary>
        public bool IsEquivalentType => (VMFlags & VMFlags.IsEquivalentType) != 0;

        /// <summary>
        /// Checks if the class has overlayed fields.
        /// </summary>
        public bool HasOverlayedFields => (VMFlags & VMFlags.HasOverlayedFields) != 0;

        /// <summary>
        /// Checks if the class has fields which must be explicitly initialized in a constructor.
        /// </summary>
        public bool HasFieldsWhichMustBeInited => (VMFlags & VMFlags.HasFieldsWhichMustBeInited) != 0;

        /// <summary>
        /// Checks if the class is an unsafe value type.
        /// </summary>
        public bool UnsafeValueType => (VMFlags & VMFlags.UnsafeValueType) != 0;

        /// <summary>
        /// Checks if the BestFitMapping and ThrowOnUnmappableChar values are initialized.
        /// </summary>
        public bool BestFitMappingInited => (VMFlags & VMFlags.BestFitMappingInited) != 0;

        /// <summary>
        /// Checks if the BestFitMapping is enabled.
        /// </summary>
        public bool BestFitMapping => (VMFlags & VMFlags.BestFitMapping) != 0;

        /// <summary>
        /// Checks if ThrowOnUnmappableChar is enabled.
        /// </summary>
        public bool ThrowOnUnmappableChar => (VMFlags & VMFlags.ThrowOnUnmappableChar) != 0;

        /// <summary>
        /// Checks if the class has no Guid.
        /// </summary>
        public bool NoGuid => (VMFlags & VMFlags.NoGuid) != 0;

        /// <summary>
        /// Checks if the class has non-public fields.
        /// </summary>
        public bool HasNonPublicFields => (VMFlags & VMFlags.HasNonPublicFields) != 0;

        /// <summary>
        /// Checks if the class contains a stack pointer.
        /// </summary>
        public bool ContainsStackPtr => (VMFlags & VMFlags.ContainsStackPtr) != 0;

        /// <summary>
        /// Checks if the class would like to have 8-byte alignment.
        /// </summary>
        public bool PreferAlign8 => (VMFlags & VMFlags.PreferAlign8) != 0;

        /// <summary>
        /// Checks if the class is sparse for COM interop.
        /// </summary>
        public bool SparseForCominterop => (VMFlags & VMFlags.SparseForCominterop) != 0;

        /// <summary>
        /// Checks if the class has a CoClass attribute.
        /// </summary>
        public bool HasCoClassAttrib => (VMFlags & VMFlags.HasCoClassAttrib) != 0;

        /// <summary>
        /// Checks if the class is a COM event interface.
        /// </summary>
        public bool IsComEventItf => (VMFlags & VMFlags.ComEventItfMask) != 0;

        /// <summary>
        /// Checks if the class is projected from WinRT.
        /// </summary>
        public bool IsProjectedFromWinRT => (VMFlags & VMFlags.ProjectedFromWinRT) != 0;

        /// <summary>
        /// Checks if the class is exported to WinRT.
        /// </summary>
        public bool IsExportedToWinRT => (VMFlags & VMFlags.ExportedToWinRT) != 0;

        /// <summary>
        /// Checks if the class is not tightly packed.
        /// </summary>
        public bool NotTightlyPacked => (VMFlags & VMFlags.NotTightlyPacked) != 0;

        /// <summary>
        /// Checks if the class contains method implementations.
        /// </summary>
        public bool ContainsMethodImpls => (VMFlags & VMFlags.ContainsMethodImpls) != 0;

        /// <summary>
        /// Checks if the marshaling type of the class is Inhibit.
        /// </summary>
        public bool IsMarshalingTypeInhibit => (VMFlags & VMFlags.MarshalingTypeMask) == VMFlags.MarshalingTypeInhibit;

        /// <summary>
        /// Checks if the marshaling type of the class is FreeThreaded.
        /// </summary>
        public bool IsMarshalingTypeFreeThreaded => (VMFlags & VMFlags.MarshalingTypeMask) == VMFlags.MarshalingTypeFreeThreaded;

        /// <summary>
        /// Checks if the marshaling type of the class is Standard.
        /// </summary>
        public bool IsMarshalingTypeStandard => (VMFlags & VMFlags.MarshalingTypeMask) == VMFlags.MarshalingTypeStandard;

        public int FieldCount
        {
            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            get
            {
                for (int i = 0; i < 99999; i++)
                {
                    if (*(FieldDesc?*)(FieldDescList + i) == null)
                        return i - 1;
                }

                return 0;
            }
        }

        /// <summary>
        /// Gets an array of FieldDescs linked with the EEClass.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public FieldDesc*[] GetFields()
        {
            int length = FieldCount;
            FieldDesc*[] fieldDescs = new FieldDesc*[length];

            for (int i = 0; i < length; i++)
                fieldDescs[i] = FieldDescList + i;
                
            return fieldDescs;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public FieldDesc* GetField(string name)
        {
            foreach (FieldDesc* fieldDesc in GetFields())
            {
                if (fieldDesc->Name == name)
                    return fieldDesc;
            }

            return null;
        }

        public ArrayClass* AsArrayClass()
        {
            fixed (EEClass* ptr = &this)
            {
                return (ArrayClass*)ptr;
            }
        }

        public DelegateEEClass* AsDelegateEEClass()
        {
            fixed (EEClass* ptr = &this)
            {
                return (DelegateEEClass*)ptr;
            }
        }
    }
}