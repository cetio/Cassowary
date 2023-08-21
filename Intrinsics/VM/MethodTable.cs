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
using Cassowary.Intrinsics.VM.EE;
using Cassowary.Reflection.Factories;
using JetBrains.Annotations;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Cassowary.Intrinsics.VM
{
    /// <summary>
    ///     The value of lowest two bits describe what the union contains
    ///     <remarks>
    ///         Use with <see cref="MethodTable.Union0Type" />
    ///     </remarks>
    /// </summary>
    [Flags]
    public enum Union0Type
    {
        EEClass = 0,
        MethodTable = 2,
        Indirection = 3,
        NullFacingEEClass = 4,
        NullFacingMethodTable = 5
    }

    [Flags]
    public enum TypeFlags : uint
    {
        Mask = 0x000F0000,
        Class = 0x00000000,
        MarshalByRefMask = 0x000E0000,
        MarshalByRef = 0x00020000,
        Contextful = 0x00030000,
        ValueType = 0x00040000,
        ValueTypeMask = 0x000C0000,
        Nullable = 0x00050000,
        PrimitiveValueType = 0x00060000,
        TruePrimitive = 0x00070000,
        Array = 0x00080000,
        ArrayMask = 0x000C0000,
        IfArrayThenSzArray = 0x00020000,
        Interface = 0x000C0000,
        TransparentProxy = 0x000E0000,
        AsyncPin = 0x000F0000,
        ElementTypeMask = 0x000E0000,
        HasFinalizer = 0x00100000,
        IfNotInterfaceThenMarshalable = 0x00200000,
        IfInterfaceThenHasGuidInfo = 0x00200000,
        ICastable = 0x00400000,
        HasIndirectParent = 0x00800000,
        ContainsPointers = 0x01000000,
        HasTypeEquivalence = 0x02000000,
        HasRCWPerTypeData = 0x04000000,
        HasCriticalFinalizer = 0x08000000,
        Collectible = 0x10000000,
        ContainsGenericVariables = 0x20000000,
        ComObject = 0x40000000,
        HasComponentSize = 0x80000000,
        NonTrivialInterfaceCast = Array | ComObject | ICastable
    }

    [Flags]
    public enum InterfaceFlags : ushort
    {
        MultipurposeSlotsMask = 0x001F,
        HasPerInstInfo = 0x0001,
        HasInterfaceMap = 0x0002,
        HasDispatchMapSlot = 0x0004,
        HasNonVirtualSlots = 0x0008,
        HasModuleOverride = 0x0010,
        IsZapped = 0x0020,
        IsPreRestored = 0x0040,
        HasModuleDependencies = 0x0080,
        IsIntrinsicType = 0x0100,
        RequiresDispatchTokenFat = 0x0200,
        HasCctor = 0x0400,
        HasCCWTemplate = 0x0800,
        RequiresAlign8 = 0x1000,
        HasBoxedRegularStatics = 0x2000,
        HasSingleNonVirtualSlot = 0x4000,
        DependsOnEquivalentOrForwardedStructs = 0x8000
    }

    [Flags]
    public enum GenericsFlags : ushort
    {
        StaticsMask = 0x00000006,
        StaticsMask_NonDynamic = 0x00000000,
        StaticsMask_Dynamic = 0x00000002,
        StaticsMask_Generics = 0x00000004,
        StaticsMask_CrossModuleGenerics = 0x00000006,
        StaticsMask_IfGenericsThenCrossModule = 0x00000002,
        NotInPZM = 0x00000008,
        GenericsMask = 0x00000030,
        GenericsMask_NonGeneric = 0x00000000,
        GenericsMask_GenericInst = 0x00000010,
        GenericsMask_SharedInst = 0x00000020,
        GenericsMask_TypicalInst = 0x00000030,
        HasRemotingVtsInfo = 0x00000080,
        HasVariance = 0x00000100,
        HasDefaultCtor = 0x00000200,
        HasPreciseInitCctors = 0x00000400,
        IsHFA = 0x00000800,
        IsRegStructPassed = 0x00000800,
        IsByRefLike = 0x00001000
    }

    [ShouldUsePointerNotObject]
    [Intrinsic]
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct MethodTable
    {
        public static MethodTable* FromType(Type type)
        {
            return (MethodTable*)type.TypeHandle.Value;
        }

        public static MethodTable* FromObject(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (obj is Array array)
            {
                MethodTable* ptr = (MethodTable*)Unsafe.Add(ref Unsafe.As<byte, nint>(ref Intrinsics.GetData(obj)), -1);
                ptr->SetArrayDesc(*(ArrayDesc*)Intrinsics.GetPointer(array));
                return ptr;
            }
            else if (obj is IEnumerable objs)
            {
                object[] objsCast = objs.Cast<object>().ToArray();
                MethodTable* ptr = (MethodTable*)Unsafe.Add(ref Unsafe.As<byte, nint>(ref Intrinsics.GetData(obj)), -1);
                ptr->SetArrayDesc(new ArrayDesc((byte)objsCast.Count(),
                    FromObject(objsCast.First())->GetClass()->LayoutEEClass->LayoutInfo.ManagedSize,
                    FromObject(objsCast)->GetClass()->Rank));
                return ptr;
            }

            return (MethodTable*)Unsafe.Add(ref Unsafe.As<byte, nint>(ref Intrinsics.GetData(obj)), -1);
        }

        /// <summary>
        /// Size of the component or type flags, depending on the context.
        /// </summary>
        [FieldOffset(0)]
        private readonly short _componentSize; // m_dwComponentSize

        /// <summary>
        /// Flags representing the type of the MethodTable.
        /// </summary>
        [FieldOffset(0)]
        public readonly TypeFlags TypeFlags; // m_eTypeFlags

        /// <summary>
        /// Flags representing generics-related information for the MethodTable.
        /// </summary>
        [FieldOffset(2)]
        public readonly GenericsFlags GenericsFlags; // m_eGenericsFlags

        /// <summary>
        /// Size of the base type represented by the MethodTable.
        /// </summary>
        [FieldOffset(4)]
        public readonly int BaseSize; // m_dwBaseSize

        /// <summary>
        /// Flags representing the interface-related information for the MethodTable.
        /// </summary>
        [FieldOffset(8)]
        public readonly InterfaceFlags InterfaceFlags; // m_eInterfaceFlags

        /// <summary>
        /// Metadata token associated with the MethodTable.
        /// </summary>
        [FieldOffset(10)]
        public readonly ushort MetadataToken; // m_wToken

        /// <summary>
        /// Number of virtual methods declared in the MethodTable.
        /// </summary>
        [FieldOffset(12)]
        public readonly ushort NumVirtuals; // m_wNumVirtuals

        /// <summary>
        /// Number of interfaces implemented by the type represented by the MethodTable.
        /// </summary>
        [FieldOffset(14)]
        public readonly ushort NumInterfaces; // m_wNumInterfaces

        /// <summary>
        /// Pointer to the parent MethodTable from which this MethodTable inherits, if any.
        /// </summary>
        [CanBeNull]
        [FieldOffset(16)]
        public readonly MethodTable* ParentMethodTable; // m_pParentMethodTable

        /// <summary>
        /// Pointer to the module that loaded the type associated with the MethodTable.
        /// </summary>
        [CanBeNull]
        [FieldOffset(24)]
        public readonly void* LoaderModule; // m_pLoaderModule

        /// <summary>
        /// Pointer to writeable data associated with the MethodTable.
        /// </summary>
        [FieldOffset(32)]
        public readonly WriteableData* WriteableData; // m_pWriteableData

        /// <summary>
        /// Part of union 0, pointer to the EEClass (cold data) shared with this MethodTable, if any.
        /// </summary>
        [CanBeNull]
        [FieldOffset(40)]
        public readonly EEClass* EEClass; // m_pEEClass

        /// <summary>
        /// Part of union 0, pointer to the canonical MethodTable that represents shared type information, if any.
        /// </summary>
        [CanBeNull]
        [FieldOffset(40)]
        public readonly MethodTable* CanonMethodTable; // m_pCanonMT

        /// <summary>
        /// Part of union 1, instance information used by the Runtime for this MethodTable.
        /// </summary>
        [CanBeNull]
        [FieldOffset(48)]
        public readonly byte* PerInstInfo; // m_pPerInstInfo

        /// <summary>
        /// Part of union 1, MethodTable to the element type owned by this MethodTable.
        /// </summary>
        [CanBeNull]
        [FieldOffset(48)]
        public readonly MethodTable* ElementMethodTable; // m_pElementMT

        /// <summary>
        /// Part of union 1, multipurpose slot 1 information used by the runtime.
        /// </summary>
        [CanBeNull]
        [FieldOffset(48)]
        public readonly byte* MultiPurposeSlot1; // m_pMultipurposeSlot1

        /// <summary>
        /// Part of union 2, pointer to the interface map of this MethodTable.
        /// </summary>
        [CanBeNull]
        [FieldOffset(56)]
        public readonly InterfaceInfo* InterfaceMap; // m_pInterfaceMap

        /// <summary>
        /// Part of union 2, multipurpose slot 2 information used by the runtime.
        /// </summary>
        [CanBeNull]
        [FieldOffset(56)]
        public readonly byte* MultiPurposeSlot2; // m_pMultipurposeSlot2

        /// <summary>
        /// Array Descriptor.
        /// </summary>
        [CanBeNull]
        [FieldOffset(64)]
        public ArrayDesc ArrayDesc;

        /// <summary>
        /// Sets the ArrayDesc for the MethodTable.
        /// </summary>
        /// <param name="arrayDesc">The ArrayDesc to set.</param>
        internal void SetArrayDesc(ArrayDesc arrayDesc)
        {
            ArrayDesc = arrayDesc;
            ArrayDesc.Contained = 1;
        }

        /// <summary>
        /// Checks if the MethodTable has an associated ArrayDesc.
        /// </summary>
        public bool HasArrayDesc
        {
            get
            {
                // This is a relatively faulty check, but it's okay because I said so.
                return ArrayDesc.Contained == 1;
            }
        }

        /// <summary>
        /// Checks if the EEClass field is truly present or not.
        /// </summary>
        public bool HasEEClass
        {
            get
            {
                try
                {
                    return !IsTruePrimitive && (Union0Type)((long)CanonMethodTable & 3) == Union0Type.EEClass && (object?)*EEClass != null;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Checks if the Canon field is truly present or not.
        /// </summary>
        public bool HasCanon
        {
            get
            {
                try
                {
                    return !IsTruePrimitive && (Union0Type)((long)CanonMethodTable & 3) == Union0Type.MethodTable && (object?)*CanonMethodTable != null;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets the component size of the MethodTable.
        /// </summary>
        public int ComponentSize
        {
            get
            {
                if (HasEEClass && EEClass->HasArrayClass)
                {
                    return _componentSize;
                }
                else if (HasArrayDesc)
                {
                    return ArrayDesc.ElemSize;
                }

                return 0;
            }
        }

        /// <summary>
        ///     Describes what the union at offset <c>40</c> points to.
        ///     contains.
        /// </summary>
        public Union0Type Union0Type
        {
            get
            {
                Union0Type unionType = (Union0Type)((long)CanonMethodTable & 3);

                if (unionType == Union0Type.EEClass && !HasEEClass)
                {
                    return Union0Type.NullFacingEEClass;
                }
                else if (unionType == Union0Type.MethodTable && !HasCanon)
                {
                    return Union0Type.NullFacingMethodTable;
                }

                return unionType;
            }
        }

        /// <summary>
        /// Checks if the MethodTable has a parent.
        /// </summary>
        public bool HasParent
        {
            get
            {
                try
                {
                    if ((nint)ParentMethodTable == 0)
                        return false;

                    // This is just intended to cause a object reference error (if the pointer doesn't point to CCWTemplate)
                    return (object?)*ParentMethodTable != null;
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool IsEnum
        {
            get
            {
                return HasParent && ParentMethodTable == FromType(typeof(Enum));
            }
        }

        public bool IsVoid
        {
            get
            {
                return AsType() == typeof(void);
            }
        }

        public bool IsString
        {
            get
            {
                return *(MethodTable*)typeof(string).TypeHandle.Value == this;
            }
        }

        public bool IsStringOrArray
        {
            get
            {
                return IsString || IsArray;
            }
        }

        public bool IsDelegate
        {
            get
            {
                return HasParent && ParentMethodTable == FromType(typeof(Delegate)) ||
                    ParentMethodTable == FromType(typeof(MulticastDelegate));
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated type is marshalable by reference.
        /// </summary>
        public bool IsMarshalByRef => (TypeFlags & TypeFlags.MarshalByRefMask) == TypeFlags.MarshalByRef;

        /// <summary>
        /// Gets a value determining whether or not the associated type is contextful.
        /// </summary>
        public bool IsContextful => TypeFlags.HasFlag(TypeFlags.Contextful);

        /// <summary>
        /// Gets a value determining whether or not the associated type is a value type.
        /// </summary>
        public bool IsValueType => (TypeFlags & TypeFlags.ValueTypeMask) == TypeFlags.ValueType;

        /// <summary>
        /// Gets a value determining whether or not the associated type is nullable.
        /// </summary>
        public bool IsNullable => TypeFlags.HasFlag(TypeFlags.Nullable);

        /// <summary>
        /// Gets a value determining whether or not the associated type is a primitive value type (enum, primitive, etc.)
        /// </summary>
        public bool IsPrimitiveValueType => TypeFlags.HasFlag(TypeFlags.PrimitiveValueType);

        /// <summary>
        /// Gets a value determining whether or not the associated type is a true primitive type.
        /// </summary>
        public bool IsTruePrimitive => TypeFlags.HasFlag(TypeFlags.TruePrimitive);

        /// <summary>
        /// Gets a value determining whether or not the associated type is an Array.
        /// </summary>
        public bool IsArray => TypeFlags.HasFlag(TypeFlags.Array);

        /// <summary>
        /// Gets a value determining whether or not the associated type is an SZArray.
        /// </summary>
        public bool IsSZArray => IsArray && TypeFlags.HasFlag(TypeFlags.IfArrayThenSzArray);

        /// <summary>
        /// Gets a value determining whether or not the associated type is an interface.
        /// </summary>
        public bool IsInterface => TypeFlags.HasFlag(TypeFlags.Interface);

        /// <summary>
        /// Gets a value determining whether or not the associated type is an interface that has GuidInfo.
        /// </summary>
        public bool IsInterfaceWithGuidInfo => TypeFlags.HasFlag(TypeFlags.IfInterfaceThenHasGuidInfo);

        /// <summary>
        /// Gets a value determining whether or not the associated type is a transparent proxy.
        /// </summary>
        public bool IsTransparentProxy => TypeFlags.HasFlag(TypeFlags.TransparentProxy);

        /// <summary>
        /// Gets a value determining whether or not the associated type is associated with an async pin.
        /// </summary>
        public bool IsAsyncPin => TypeFlags.HasFlag(TypeFlags.AsyncPin);

        /// <summary>
        /// Gets a value determining whether or not the associated type has a finalizer.
        /// </summary>
        public bool HasFinalizer => TypeFlags.HasFlag(TypeFlags.HasFinalizer);

        /// <summary>
        /// Gets a value determining whether or not the associated type is an ICastable.
        /// </summary>
        public bool IsICastable => TypeFlags.HasFlag(TypeFlags.ICastable);

        /// <summary>
        /// Gets a value determining whether or not the associated type has an indirect parent.
        /// </summary>
        public bool HasIndirectParent => TypeFlags.HasFlag(TypeFlags.HasIndirectParent);

        /// <summary>
        /// Gets a value determining whether or not the associated type contains any pointers.
        /// </summary>
        public bool ContainsPointers => TypeFlags.HasFlag(TypeFlags.ContainsPointers);

        /// <summary>
        /// Gets a value determining whether or not the associated type is equivalent to any other types.
        /// </summary>
        public bool HasTypeEquivalence => TypeFlags.HasFlag(TypeFlags.HasTypeEquivalence);

        /// <summary>
        /// Gets a value determining whether or not the associated type has RCW per-type data.
        /// </summary>
        public bool HasRCWPerTypeData => TypeFlags.HasFlag(TypeFlags.HasRCWPerTypeData);

        /// <summary>
        /// Gets a value determining whether or not the associated type has a critical finalizer that must be executed.
        /// </summary>
        public bool HasCriticalFinalizer => TypeFlags.HasFlag(TypeFlags.HasCriticalFinalizer);

        /// <summary>
        /// Gets a value determining whether or not the associated type is collectible.
        /// </summary>
        public bool IsCollectible => TypeFlags.HasFlag(TypeFlags.Collectible);

        /// <summary>
        /// Gets a value determining whether or not the associated type contains any generic variables.
        /// </summary>
        public bool ContainsGenericVariables => TypeFlags.HasFlag(TypeFlags.ContainsGenericVariables);

        /// <summary>
        /// Gets a value determining whether or not the associated type is a COM object.
        /// </summary>
        public bool IsCOMObject => TypeFlags.HasFlag(TypeFlags.ComObject);

        /// <summary>
        /// Gets a value determining whether or not the associated type has a component size, like an Array.
        /// </summary>
        public bool HasComponentSize => TypeFlags.HasFlag(TypeFlags.HasComponentSize);

        /// <summary>
        /// Gets a value determining whether or not the associated type has a non-trivial interface cast.
        /// </summary>
        public bool HasNonTrivialInterfaceCast => TypeFlags.HasFlag(TypeFlags.NonTrivialInterfaceCast);

        /// <summary>
        /// Gets a value determining whether or not the associated type is a static dynamic.
        /// </summary>
        public bool IsStaticsDynamic => (GenericsFlags & GenericsFlags.StaticsMask) == GenericsFlags.StaticsMask_Dynamic;

        /// <summary>
        /// Gets a value determining whether or not the associated type is not in it's preferred zap module.
        /// </summary>
        public bool IsNotInPZM => GenericsFlags.HasFlag(GenericsFlags.NotInPZM);

        /// <summary>
        /// GGets a value determining whether or not the associated type is a generic instance.
        /// </summary>
        public bool IsGenericInst => (GenericsFlags & GenericsFlags.GenericsMask) == GenericsFlags.GenericsMask_GenericInst;

        /// <summary>
        /// Gets a value determining whether or not the associated type is a shared instance.
        /// </summary>
        public bool IsSharedInst => (GenericsFlags & GenericsFlags.GenericsMask) == GenericsFlags.GenericsMask_SharedInst;

        /// <summary>
        /// Gets a value determining whether or not the associated type is a typical instance.
        /// </summary>
        public bool IsTypicalInst => (GenericsFlags & GenericsFlags.GenericsMask) == GenericsFlags.GenericsMask_TypicalInst;

        /// <summary>
        /// Gets a value determining whether or not the associated type has remoting VTS information.
        /// </summary>
        public bool HasRemotingVtsInfo => GenericsFlags.HasFlag(GenericsFlags.HasRemotingVtsInfo);

        /// <summary>
        /// Gets a value determining whether or not the associated type has variance.
        /// </summary>
        public bool HasVariance => GenericsFlags.HasFlag(GenericsFlags.HasVariance);

        /// <summary>
        /// Gets a value determining whether or not the associated type has a default constuctor.
        /// </summary>
        public bool HasDefaultCtor => GenericsFlags.HasFlag(GenericsFlags.HasDefaultCtor);

        /// <summary>
        /// Gets a value determining whether or not the associated type has precise init constructors.
        /// </summary>
        public bool HasPreciseInitCtors => GenericsFlags.HasFlag(GenericsFlags.HasPreciseInitCctors);

        /// <summary>
        /// Gets a value determining whether or not the associated type is an HFA (homogeneous floating-point aggregates).
        /// </summary>
        public bool IsHFA => GenericsFlags.HasFlag(GenericsFlags.IsHFA);

        /// <summary>
        /// Gets a value determining whether or not the associated type is a reference to a regular structure.
        /// </summary>
        public bool IsRegStructPassed => GenericsFlags.HasFlag(GenericsFlags.IsRegStructPassed);

        /// <summary>
        /// Gets a value determining whether or not the associated type is passed like a by-ref.
        /// </summary>
        public bool IsByRefLike => GenericsFlags.HasFlag(GenericsFlags.IsByRefLike);

        /// <summary>
        /// Gets a value determining whether or not the associated type has any per-instance information.
        /// </summary>
        public bool HasPerInstInfo => InterfaceFlags.HasFlag(InterfaceFlags.HasPerInstInfo);

        /// <summary>
        /// Gets a value determining whether or not the associated type has an InterfaceMap, the map length == NumInterfaces.
        /// </summary>
        public bool HasInterfaceMap => InterfaceFlags.HasFlag(InterfaceFlags.HasInterfaceMap);

        /// <summary>
        /// Gets a value determining whether or not the associated type has a dispatch map slot.
        /// </summary>
        public bool HasDispatchMapSlot => InterfaceFlags.HasFlag(InterfaceFlags.HasDispatchMapSlot);

        /// <summary>
        /// Gets a value determining whether or not the associated type has any non-virtual slots.
        /// </summary>
        public bool HasNonVirtualSlots => InterfaceFlags.HasFlag(InterfaceFlags.HasNonVirtualSlots);

        /// <summary>
        /// Gets a value determining whether or not the associated type has module overrides.
        /// </summary>
        public bool HasModuleOverride => InterfaceFlags.HasFlag(InterfaceFlags.HasModuleOverride);

        /// <summary>
        /// Gets a value determining whether or not the associated type is zapped.
        /// </summary>
        public bool IsZapped => InterfaceFlags.HasFlag(InterfaceFlags.IsZapped);

        /// <summary>
        /// Gets a value determining whether or not the associated type is pre-restored.
        /// </summary>
        public bool IsPreRestored => InterfaceFlags.HasFlag(InterfaceFlags.IsPreRestored);

        /// <summary>
        /// Gets a value determining whether or not the associated type depends on an external module.
        /// </summary>
        public bool HasModuleDependencies => InterfaceFlags.HasFlag(InterfaceFlags.HasModuleDependencies);

        /// <summary>
        /// Gets a value determining whether or not the associated type is intrinsic.
        /// </summary>
        public bool IsIntrinsicType => InterfaceFlags.HasFlag(InterfaceFlags.IsIntrinsicType);

        /// <summary>
        /// Gets a value determining whether or not the associated type requires a fat dispatch token.
        /// </summary>
        public bool RequiresDispatchTokenFat => InterfaceFlags.HasFlag(InterfaceFlags.RequiresDispatchTokenFat);

        /// <summary>
        /// Gets a value determining whether or not the associated type has any constructor.
        /// </summary>
        public bool HasCctor => InterfaceFlags.HasFlag(InterfaceFlags.HasCctor);

        /// <summary>
        /// Gets a value determining whether or not the associated type has a CCWTemplate, this only happens with COM exposed types.
        /// </summary>
        public bool HasCCWTemplate => InterfaceFlags.HasFlag(InterfaceFlags.HasCCWTemplate);

        /// <summary>
        /// Gets a value determining whether or not the associated type requires alignment of 8 bytes in memory.
        /// </summary>
        public bool RequiresAlign8 => InterfaceFlags.HasFlag(InterfaceFlags.RequiresAlign8);

        /// <summary>
        /// Gets a value determining whether or not the associated type has boxed regular statics.
        /// </summary>
        public bool HasBoxedRegularStatics => InterfaceFlags.HasFlag(InterfaceFlags.HasBoxedRegularStatics);

        /// <summary>
        /// Gets a value determining whether or not the associated type only has one non-virtual slot available.
        /// </summary>
        public bool HasSingleNonVirtualSlot => InterfaceFlags.HasFlag(InterfaceFlags.HasSingleNonVirtualSlot);

        /// <summary>
        /// Gets a value determining whether or not the associated type depends on equivalent or forwarded structs.
        /// </summary>
        public bool DependsOnEquivalentOrForwardedStructs => InterfaceFlags.HasFlag(InterfaceFlags.DependsOnEquivalentOrForwardedStructs);

        /// <summary>
        /// Gets the EEClass of this MethodTable, even if it does not have one in its union.
        /// </summary>
        /// <returns>The EEClass of this MethodTable.</returns>
        public EEClass* GetClass()
        {
            if (HasEEClass)
            {
                return EEClass;
            }
            else if (HasCanon)
            {
                return CanonMethodTable->GetClass();
            }

            // This should never happen.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the root MethodTable from the Canonical MethodTable.
        /// </summary>
        /// <returns>Root MethodTable.</returns>
        public MethodTable* RootCanonTable()
        {
            fixed (MethodTable* ptr = &this)
            {
                if (!HasCanon)
                {
                    return ptr;
                }

                return CanonMethodTable->RootCanonTable();
            }
        }

        // The type nor method should ever be null, unless they're removed.
        private static Func<nint, object> AllocateInternal = Unsafe.As<Func<nint, object>>(DelegateFactory.MakeDelegate(Type.GetType("System.StubHelpers.StubHelpers")!.GetMethod("AllocateInternal", BindingFlags.NonPublic | BindingFlags.Static)!));

        /// <summary>
        /// Checks if this MethodTable can allocate an object.
        /// </summary>
        /// <returns>True if if this MethodTable can allocate an object, otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanAllocate()
        {
            return !IsVoid && !IsInterface;
        }

        /// <summary>
        /// Allocates this MethodTable, checks if <see cref="MethodTable.CanAllocate"/> returns true.
        /// </summary>
        /// <returns>The allocated object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public object Allocate()
        {
            if (!CanAllocate())
                throw new ArgumentException($"{this} does not support allocation."); ;

            // This is okay because we already checked if we can allocate.
            return AllocateNoChecks();
        }

        /// <summary>
        /// Allocates this MethodTable, if it is an Array.
        /// </summary>
        /// <param name="lengths">The lengths of the Array to allocate.</param>
        /// <returns>The allocated object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public Array AllocateArray(params int[] lengths)
        {
            if (!IsArray)
                throw new ArgumentException($"{this} is not an array type, and cannot allocate using AllocateArray()");

            if (lengths.Length == 0)
                return Array.CreateInstance(ElementMethodTable->AsType(), 1);

            return Array.CreateInstance(ElementMethodTable->AsType(), lengths);
        }

        /// <summary>
        /// Allocates this MethodTable, if it is an Array.
        /// </summary>
        /// <param name="length">The length of the first known rank, or only rank.</param>
        /// <returns>The allocated object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public Array AllocateArrayUnknownRank(int length)
        {
            if (!IsArray)
                throw new ArgumentException($"{this} is not an array type, and cannot allocate using AllocateArray()");

            int[] bounds = new int[GetClass()->AsArrayClass()->Rank];
            
            for (int i = 0; i < GetClass()->AsArrayClass()->Rank; i++)
                bounds[i] = length;
            
            return Array.CreateInstance(ElementMethodTable->AsType(), bounds);
        }

        /// <summary>
        /// Allocates and returns an object of the specified type, does not check if <see cref="MethodTable.CanAllocate"/> returns true.
        /// </summary>
        /// <returns>The allocated object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public object AllocateNoChecks()
        {
            if (IsString)
            {
                return string.Empty;
            }
            if (IsArray)
            {
                return AllocateArray();
            }
            else
            {
                fixed (MethodTable* ptr = &this)
                {
                    return AllocateInternal((nint)ptr);
                }
            }
        }

        /// <summary>
        /// Checks if this MethodTable can box from a pointer.
        /// </summary>
        /// <returns>True if if this MethodTable can box from a pointer, otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanBox()
        {
            return !IsVoid && !ContainsGenericVariables && !IsGenericInst;
        }

        /// <summary>
        /// Checks if this MethodTable can box strictly from a pointer.
        /// </summary>
        /// <returns>True if if this MethodTable can box strictly from a pointer, otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanBoxStrict()
        {
            return CanBox() && !ContainsPointers && !IsByRefLike && !IsAsyncPin && !IsInterface;
        }

        /// <summary>
        /// Boxes a value from a pointer of a specified type, checks if <see cref="CanBox"/> returns true.
        /// </summary>
        /// <param name="ptr">A pointer to the value.</param>
        /// <returns>The boxed object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public object Box(void* ptr)
        {
            if (!CanBox())
                throw new ArgumentException($"{this} does not support boxing.");

            // This is okay because we already checked if the boxing is allowed.
            return BoxNoChecks(ptr);
        }

        /// <summary>
        /// Strictly boxes a value from a pointer of a specified type, checks if <see cref="CanBoxStrict"/> returns true.
        /// </summary>
        /// <param name="ptr">A pointer to the value.</param>
        /// <returns>The boxed object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public object BoxStrict(void* ptr)
        {
            if (!CanBoxStrict())
                throw new ArgumentException($"{this} does not support strict boxing.");

            // This is okay because we already checked if the boxing is allowed.
            return BoxNoChecks(ptr);
        }

        /// <summary>
        /// Boxes a value from a pointer of a specified type, does not check if <see cref="CanBox"/> or <see cref="CanBoxStrict"/> returns true.
        /// </summary>
        /// <param name="ptr">A pointer to the value.</param>
        /// <returns>The boxed object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public object BoxNoChecks(void* ptr)
        {
            // ok, maybe some checks
            if (IsString)
            {
                return new string((char*)Unsafe.Add(ptr, 4), 0, *(int*)ptr);
            }
            if (IsArray)
            {
                Array array = AllocateArrayUnknownRank(*(int*)ptr);
                Unsafe.CopyBlock(Intrinsics.GetPointer(array), ptr, (uint)(*(int*)ptr * ComponentSize) + 8);
                return array;
            }
            else
            {
                object obj = AllocateNoChecks();
                Unsafe.Copy(ref Intrinsics.GetData(obj), ptr);
                return obj;
            }
        }

        /// <summary>
        /// Checks if this MethodTable can cast an object to the given Interface MethodTable.
        /// </summary>
        /// <param name="pMT">The MethodTable to check if can be casted to.</param>
        /// <returns>True if this MethodTable can cast to the given MethodTable, otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public bool CanCastToInterface(MethodTable* pMT)
        {
            return GetInterfaces().Any(x => x.MethodTable == pMT);
        }

        /// <summary>
        /// Checks if this MethodTable can cast an object to the given MethodTable.
        /// </summary>
        /// <param name="pMT">The MethodTable to check if can be casted to.</param>
        /// <returns>True if this MethodTable can cast to the given MethodTable, otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public bool CanCastTo(MethodTable* pMT)
        {
            if (pMT->IsGenericInst == IsGenericInst &&
                pMT->IsStringOrArray == IsStringOrArray &&
                pMT->NumVirtuals == NumVirtuals &&
                pMT->IsNullable == IsNullable &&
                pMT->IsArray == IsArray)
            {
                if (pMT->IsArray && pMT->GetClass()->Rank != GetClass()->Rank)
                {
                    return false;
                }
                else if (pMT->IsInterface)
                {
                    return CanCastToInterface(pMT);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Casts the given object to this MethodTable, checks if <see cref="MethodTable.CanCastTo"/> returns true.
        /// </summary>
        /// <param name="obj">The object to cast to the provided MethodTable.</param>
        /// <returns>Obj as pMT.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public object Cast(object obj)
        {
            fixed (MethodTable* pMT = &this)
            {
                MethodTable* poMT = FromObject(obj);

                if (poMT == pMT)
                    return obj;

                if (!poMT->CanCastTo(pMT))
                    throw new ArgumentException($"{*poMT} cannot cast to {*pMT}");

                // This is okay because we already checked if the cast can happen.
                return CastNoChecks(obj);
            }
        }

        /// <summary>
        /// Casts the given object to this MethodTable, does not check if <see cref="MethodTable.CanCastTo"/> returns true.
        /// </summary>
        /// <param name="obj">The object to cast to the provided MethodTable.</param>
        /// <returns>Obj as pMT.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public object CastNoChecks(object obj)
        {
            fixed (MethodTable* pMT = &this)
            {
                // Copy should never return as null, so this is fine.
                object copy = null!;

                if (pMT->IsString)
                {
                    return obj.ToString() ?? string.Empty;
                }
                else if (pMT->IsArray)
                {
                    MethodTable* poMT = FromObject(obj);

                    if (poMT->IsArray)
                        return pMT->BoxNoChecks(Intrinsics.GetPointer(obj));
                    
                    copy = pMT->AllocateArray();
                    Unsafe.CopyBlock(Unsafe.Add(Intrinsics.GetPointer(copy), 8), Unsafe.Add(Intrinsics.GetPointer(obj), 8), (uint)ComponentSize);
                    return copy;
                }
                else
                {
                    copy = pMT->AllocateNoChecks();
                    Unsafe.CopyBlock(ref Intrinsics.GetData(copy), ref Intrinsics.GetData(obj), (uint)pMT->BaseSize);
                    return copy;
                }
            }
        }

        /// <summary>
        /// Constructs an object of this MethodTable with the given parameters.
        /// </summary>
        /// <param name="obj">The object to be constructed.</param>
        /// <param name="parameters">The parameters for the object's constructor.</param>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public void Construct(object obj, params object[] parameters)
        {
            Type[] parameterTypes = new Type[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
                parameterTypes[i] = parameters[i].GetType();

            ConstructorInfo ctorInfo = AsType().GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, parameterTypes);
            dynamic rtCtorInfo = Intrinsics.AsRuntimeConstructorInfo(ctorInfo);

            rtCtorInfo.Invoke(obj, BindingFlags.Default, null, parameters, null);
        }


        /// <summary>
        /// Constructs this MethodTable with the given parameters.
        /// </summary>
        /// <param name="parameters">The parameters for the object's constructor.</param>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public object Construct(params object[] parameters)
        {
            Type[] parameterTypes = new Type[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
                parameterTypes[i] = parameters[i].GetType();

            ConstructorInfo ctorInfo = AsType().GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, parameterTypes);
            dynamic rtCtorInfo = Intrinsics.AsRuntimeConstructorInfo(ctorInfo);

            return rtCtorInfo.Invoke(BindingFlags.Default, null, parameters, null);
        }

        /// <summary>
        /// Gets all interfaces that this MethodTable has.
        /// </summary>
        /// <returns>All interfaces that this MethodTable has.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InterfaceInfo[] GetInterfaces()
        {
            if (!HasInterfaceMap)
                return new InterfaceInfo[0];

            InterfaceInfo[] interfaces = new InterfaceInfo[NumInterfaces];

            for (int i = 0; i < NumInterfaces; i++)
                interfaces[i] = *(InterfaceMap + i);

            return interfaces;
        }

        /// <summary>
        /// Gets the type of the specified pointer.
        /// </summary>
        /// <param name="ptr">The pointer to get the type of.</param>
        /// <returns>The type of the specified pointer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Type AsType()
        {
            fixed (MethodTable* ptr = &this)
            {
                // If this MethodTable exists then, clearly, the Type must also exist.
                return Type.GetTypeFromHandle(RuntimeTypeHandle.FromIntPtr((nint)ptr)!)!;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MethodTable* AsByRefTable()
        {
            return FromType(AsType().MakeByRefType());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MethodTable* AsArrayTable()
        {
            return FromType(AsType().MakeArrayType());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MethodTable* AsPointerTable()
        {
            return FromType(AsType().MakePointerType());
        }

        public static bool operator ==(MethodTable methodTable, object obj)
        {
            if (obj is not MethodTable)
                return false;

            return methodTable.Equals(obj);
        }

        public static bool operator !=(MethodTable methodTable, object obj)
        {
            if (obj is not MethodTable)
                return true;

            return !methodTable.Equals(obj);
        }

        public override string ToString()
        {
            return AsType().FullName ?? string.Empty;
        }
    }
}
