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
using System.Runtime.Intrinsics;
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
        public readonly CorInterfaceType CorInterfaceType; // m_ComInterfaceType

        /// <summary>
        /// Contains the CCWTemplate if the EEClass is associated with a COM Visible type.
        /// </summary>
        [CanBeNull]
        [FieldOffset(48)]
        public readonly CCWTemplate* CCWTemplate; // m_pccwTemplate

        [FieldOffset(56)]
        public readonly TypeAttributes TypeAttributes; // m_dwAttrClass

        [FieldOffset(60)]
        public readonly VMFlags VMFlags; // m_VMFlags

        [FieldOffset(64)]
        public readonly CorElementType CorElementType; // m_NormType

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
        /// If the type is not an array, the rank will be 0.
        /// </remarks>
        public int Rank
        {
            get
            {
                if (HasArrayClass)
                    return AsArrayClass()->Rank;

                if (MethodTable->HasArrayDesc)
                    return MethodTable->ArrayDesc.Rank;

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
                if (MethodTable->HasArrayDesc)
                    return LayoutEEClass->LayoutInfo.ManagedSize + MethodTable->ComponentSize * MethodTable->ArrayDesc.Length;

                if (HasArrayClass)
                    return MethodTable->ComponentSize * Rank;

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
        public bool HasGuidInfo
        {
            get
            {
                return !NoGuid && MethodTable->IsInterfaceWithGuidInfo;
            }
        }

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

        /// <summary>
        /// Gets a value determining whether or not the associated type has an ArrayClass.
        /// </summary>
        public bool HasArrayClass
        {
            get
            {
                return CorElementType == CorElementType.ELEMENT_TYPE_ARRAY || CorElementType == CorElementType.ELEMENT_TYPE_SZARRAY;
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type has a DelegateEEClass.
        /// </summary>
        public bool HasDelegateEEClass
        {
            get
            {
                return IsDelegate;
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type is a primitive type.
        /// </summary>
        public bool IsPrimitive
        {
            get
            {
                return Intrinsics.CorIsPrimitiveType(CorElementType);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type uses CharSet.Ansi.
        /// </summary>
        public bool IsAnsiClass
        {
            get
            {
                return TypeAttributes.HasFlag(TypeAttributes.AnsiClass);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type has auto layout.
        /// </summary>
        public bool HasAutoLayout
        {
            get
            {
                return TypeAttributes.HasFlag(TypeAttributes.AutoLayout);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type is an interface.
        /// </summary>
        public bool IsInterface
        {
            get
            {
                return MethodTable->IsInterface;
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type is a class.
        /// </summary>
        public bool IsClass
        {
            get
            {
                return TypeAttributes.HasFlag(TypeAttributes.Class);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type is not a public type.
        /// </summary>
        public bool IsNotPublic
        {
            get
            {
                return TypeAttributes.HasFlag(TypeAttributes.NotPublic);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type is a public type.
        /// </summary>
        public bool IsPublic
        {
            get
            {
                return TypeAttributes.HasFlag(TypeAttributes.Public);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type is a nested public type.
        /// </summary>
        public bool IsNestedPublic
        {
            get
            {
                return TypeAttributes.HasFlag(TypeAttributes.NestedPublic);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type is a nested private type.
        /// </summary>
        public bool IsNestedPrivate
        {
            get
            {
                return TypeAttributes.HasFlag(TypeAttributes.NestedPrivate);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type is a nested family type.
        /// </summary>
        public bool IsNestedFamily
        {
            get
            {
                return TypeAttributes.HasFlag(TypeAttributes.NestedFamily);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type is a nested assembly type.
        /// </summary>
        public bool IsNestedAssembly
        {
            get
            {
                return TypeAttributes.HasFlag(TypeAttributes.NestedAssembly);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type is a nested family and assembly type.
        /// </summary>
        public bool IsNestedFamANDAssem
        {
            get
            {
                return TypeAttributes.HasFlag(TypeAttributes.NestedFamANDAssem);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type is a nested family or assembly type.
        /// </summary>
        public bool IsNestedFamORAssem
        {
            get
            {
                return TypeAttributes.HasFlag(TypeAttributes.NestedFamORAssem);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type has sequential layout.
        /// </summary>
        public bool HasSequentialLayout
        {
            get
            {
                return TypeAttributes.HasFlag(TypeAttributes.SequentialLayout);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type has explicit layout.
        /// </summary>
        public bool HasExplicitLayout
        {
            get
            {
                return TypeAttributes.HasFlag(TypeAttributes.ExplicitLayout);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type is abstract.
        /// </summary>
        public bool IsAbstract
        {
            get
            {
                return TypeAttributes.HasFlag(TypeAttributes.Abstract);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type is sealed.
        /// </summary>
        public bool IsSealed
        {
            get
            {
                return TypeAttributes.HasFlag(TypeAttributes.Sealed);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type is a special name.
        /// </summary>
        public bool IsSpecialName
        {
            get
            {
                return TypeAttributes.HasFlag(TypeAttributes.SpecialName);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type is a runtime special name.
        /// </summary>
        public bool IsRTSpecialName
        {
            get
            {
                return TypeAttributes.HasFlag(TypeAttributes.RTSpecialName);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type is an import.
        /// </summary>
        public bool IsImport
        {
            get
            {
                return TypeAttributes.HasFlag(TypeAttributes.Import);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type is serializable.
        /// </summary>
        public bool IsSerializable
        {
            get
            {
                return TypeAttributes.HasFlag(TypeAttributes.Serializable);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type is a Windows runtime component.
        /// </summary>
        public bool IsWindowsRuntime
        {
            get
            {
                return TypeAttributes.HasFlag(TypeAttributes.WindowsRuntime);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type uses CharSet.Unicode.
        /// </summary>
        public bool IsUnicodeClass
        {
            get
            {
                return TypeAttributes.HasFlag(TypeAttributes.UnicodeClass);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type uses CharSet.Auto.
        /// </summary>
        public bool IsAutoClass
        {
            get
            {
                return TypeAttributes.HasFlag(TypeAttributes.AutoClass);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type has a custom format.
        /// </summary>
        public bool HasCustomFormat
        {
            get
            {
                return TypeAttributes.HasFlag(TypeAttributes.CustomFormatClass);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type has security.
        /// </summary>
        public bool HasSecurity
        {
            get
            {
                return TypeAttributes.HasFlag(TypeAttributes.HasSecurity);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type layout depends on other modules.
        /// </summary>
        public bool LayoutDependsOnOtherModules
        {
            get
            {
                return VMFlags.HasFlag(VMFlags.LayoutDependsOnOtherModules);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type is a delegate.
        /// </summary>
        public bool IsDelegate
        {
            get
            {
                return VMFlags.HasFlag(VMFlags.Delegate);
            }
        }

        /// <summary>
        /// Checks if value type statics in this class will be pinned.
        /// </summary>
        public bool FixedAddressVtStatics
        {
            get
            {
                return VMFlags.HasFlag(VMFlags.FixedAddressVtStatics);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type has layout information.
        /// </summary>
        public bool HasLayout
        {
            get
            {
                return VMFlags.HasFlag(VMFlags.HasLayout);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type is nested.
        /// </summary>
        public bool IsNested
        {
            get
            {
                return VMFlags.HasFlag(VMFlags.IsNested);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type is an equivalent type.
        /// </summary>
        public bool IsEquivalentType
        {
            get
            {
                return VMFlags.HasFlag(VMFlags.IsEquivalentType);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type has overlayed fields.
        /// </summary>
        public bool HasOverlayedFields
        {
            get
            {
                return VMFlags.HasFlag(VMFlags.HasOverlayedFields);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type has fields which must be explicitly initialized in a constructor.
        /// </summary>
        public bool HasFieldsWhichMustBeInited
        {
            get
            {
                return VMFlags.HasFlag(VMFlags.HasFieldsWhichMustBeInited);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type is an unsafe value type.
        /// </summary>
        public bool UnsafeValueType
        {
            get
            {
                return VMFlags.HasFlag(VMFlags.UnsafeValueType);
            }
        }

        /// <summary>
        /// Checks if the BestFitMapping and ThrowOnUnmappableChar values are initialized.
        /// </summary>
        public bool BestFitMappingInited
        {
            get
            {
                return VMFlags.HasFlag(VMFlags.BestFitMappingInited);
            }
        }

        /// <summary>
        /// Checks if the BestFitMapping is enabled.
        /// </summary>
        public bool BestFitMapping
        {
            get
            {
                return VMFlags.HasFlag(VMFlags.BestFitMapping);
            }
        }

        /// <summary>
        /// Checks if ThrowOnUnmappableChar is enabled.
        /// </summary>
        public bool ThrowOnUnmappableChar
        {
            get
            {
                return VMFlags.HasFlag(VMFlags.ThrowOnUnmappableChar);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type has no Guid.
        /// </summary>
        public bool NoGuid
        {
            get
            {
                return VMFlags.HasFlag(VMFlags.NoGuid);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type has non-public fields.
        /// </summary>
        public bool HasNonPublicField
        {
            get
            {
                return VMFlags.HasFlag(VMFlags.HasNonPublicFields);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type contains a stack pointer.
        /// </summary>
        public bool ContainsStackPtr
        {
            get
            {
                return VMFlags.HasFlag(VMFlags.ContainsStackPtr);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type would like to have 8-byte alignment.
        /// </summary>
        public bool PreferAlign8
        {
            get
            {
                return VMFlags.HasFlag(VMFlags.PreferAlign8);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type is sparse for COM interop.
        /// </summary>
        public bool SparseForCominterop
        {
            get
            {
                return VMFlags.HasFlag(VMFlags.SparseForCominterop);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type has a CoClass attribute.
        /// </summary>
        public bool HasCoClassAttrib
        {
            get
            {
                return VMFlags.HasFlag(VMFlags.HasCoClassAttrib);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type is a COM event interface.
        /// </summary>
        public bool IsComEventItf
        {
            get
            {
                return VMFlags.HasFlag(VMFlags.ComEventItfMask);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type is projected from WinRT.
        /// </summary>
        public bool IsProjectedFromWinRT
        {
            get
            {
                return VMFlags.HasFlag(VMFlags.ProjectedFromWinRT);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type is exported to WinRT.
        /// </summary>
        public bool IsExportedToWinRT
        {
            get
            {
                return VMFlags.HasFlag(VMFlags.ExportedToWinRT);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type is not tightly packed.
        /// </summary>
        public bool NotTightlyPacked
        {
            get
            {
                return VMFlags.HasFlag(VMFlags.NotTightlyPacked);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type contains method implementations.
        /// </summary>
        public bool ContainsMethodImpls
        {
            get
            {
                return VMFlags.HasFlag(VMFlags.ContainsMethodImpls);
            }
        }

        /// <summary>
        /// Checks if the marshaling type of the class is Inhibit.
        /// </summary>
        public bool IsMarshalingTypeInhibit
        {
            get
            {
                return (VMFlags & VMFlags.MarshalingTypeMask) == VMFlags.MarshalingTypeInhibit;
            }
        }

        /// <summary>
        /// Checks if the marshaling type of the class is FreeThreaded.
        /// </summary>
        public bool IsMarshalingTypeFreeThreaded
        {
            get
            {
                return (VMFlags & VMFlags.MarshalingTypeMask) == VMFlags.MarshalingTypeFreeThreaded;
            }
        }

        /// <summary>
        /// Checks if the marshaling type of the class is Standard.
        /// </summary>
        public bool IsMarshalingTypeStandard
        {
            get
            {
                return (VMFlags & VMFlags.MarshalingTypeMask) == VMFlags.MarshalingTypeStandard;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public int GetFieldCount()
        {
            if (ManagedSize == 0)
                return 0;

            for (int i = 0; i < 99999; i++)
            {
                if (*(FieldDesc?*)(FieldDescList + i) == null)
                    return i - 1;
            }

            return 0;
        }

        /// <summary>
        /// Gets an array of FieldDescs linked with the EEClass.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public FieldDesc*[] GetFields()
        {
            int length = GetFieldCount();
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

        public GuidInfo* GetOrMakeGuid()
        {
            if (HasGuidInfo)
                return GuidInfo;

            GuidInfo guidInfo = new GuidInfo(MethodTable->AsType().GUID, true);
            return &guidInfo;
        }

        public int GetVectorSize()
        {
            if (MethodTable->AsType() == typeof(System.Numerics.Vector))
            {
                return MethodTable->NumInstanceFieldBytes;
            }
            else if (MethodTable->AsType() == typeof(Vector64))
            {
                return 8;
            }
            else if (MethodTable->AsType() == typeof(Vector128))
            {
                return 16;
            }
            else if (MethodTable->AsType() == typeof(Vector256))
            {
                return 32;
            }

            return 0;
        }

        public CorInfoHFAElemType GetHFAElemType()
        {
            int vectorSize = GetVectorSize();

            if (vectorSize == 8)
            {
                return CorInfoHFAElemType.CORINFO_HFA_ELEM_VECTOR64;
            }
            else if (vectorSize == 16)
            {
                return CorInfoHFAElemType.CORINFO_HFA_ELEM_VECTOR128;
            }
            else if (vectorSize == 32)
            {
                return CorInfoHFAElemType.CORINFO_HFA_ELEM_VECTOR256;
            }

            FieldDesc* fieldDesc = GetFields()[0];

            if (fieldDesc->CorElementType == CorElementType.ELEMENT_TYPE_R4)
            {
                return CorInfoHFAElemType.CORINFO_HFA_ELEM_FLOAT;
            }
            else if (fieldDesc->CorElementType == CorElementType.ELEMENT_TYPE_R8)
            {
                return CorInfoHFAElemType.CORINFO_HFA_ELEM_DOUBLE;
            }

            return CorInfoHFAElemType.CORINFO_HFA_ELEM_NONE;
        }

        public CharSet GetCharSet()
        {
            if (IsAnsiClass)
                return CharSet.Ansi;

            if (IsUnicodeClass)
                return CharSet.Unicode;

            if (IsAutoClass)
                return CharSet.Auto;

            return CharSet.None;
        }
    }
}