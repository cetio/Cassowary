using Cassowary.Attributes;
using Cassowary.Runtime.Cor;
using Cassowary.Runtime.VM.EE;
using Cassowary.Signatures;
using JetBrains.Annotations;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Cassowary.Runtime.VM
{
    /// <summary>
    ///     The value of the lowest bit of the union at offset 40 describes what it contains
    ///     (.NET 8 CoreCLR: UNION_EECLASS = 0, UNION_METHODTABLE = 1, UNION_MASK = 1).
    /// </summary>
    public enum Union0Type
    {
        /// <summary>Pointer to the EEClass, this MethodTable is canonical.</summary>
        EEClass = 0,
        /// <summary>Pointer to the canonical MethodTable (tagged with low bit).</summary>
        MethodTable = 1
    }

    /// <summary>
    ///     High 16 bits of the native m_dwFlags field (.NET 8 WFLAGS_HIGH_ENUM).
    ///     The low 16 bits are component size (arrays/strings) or the generics flags,
    ///     see <see cref="VM.GenericsFlags"/>.
    /// </summary>
    [Flags]
    public enum TypeFlags : uint
    {
        /// <summary>Category mask.</summary>
        Mask = 0x000F0000,
        Class = 0x00000000,
        ValueType = 0x00040000,
        ValueTypeMask = 0x000C0000,
        Nullable = 0x00050000,
        PrimitiveValueType = 0x00060000,
        TruePrimitive = 0x00070000,
        Array = 0x00080000,
        ArrayMask = 0x000C0000,
        IfArrayThenSzArray = 0x00020000,
        Interface = 0x000C0000,
        ElementTypeMask = 0x000E0000,
        HasFinalizer = 0x00100000,
        /// <summary>Class implements IDynamicInterfaceCastable.</summary>
        IDynamicInterfaceCastable = 0x00200000,
        ICastable = 0x00400000,
        /// <summary>0x00800000 is unused in .NET 8.</summary>
        Unused_1 = 0x00800000,
        ContainsPointers = 0x01000000,
        HasTypeEquivalence = 0x02000000,
        /// <summary>Type is a tracked reference with a finalizer.</summary>
        IsTrackedReferenceWithFinalizer = 0x04000000,
        HasCriticalFinalizer = 0x08000000,
        Collectible = 0x10000000,
        ContainsGenericVariables = 0x20000000,
        ComObject = 0x40000000,
        HasComponentSize = 0x80000000,
        /// <summary>Any of the non-trivial-interface-cast bits (masked-any check, not equality).</summary>
        NonTrivialInterfaceCast = Array | ComObject | ICastable | IDynamicInterfaceCastable | ValueType
    }

    /// <summary>
    ///     m_wFlags2 of the native MethodTable (offset 8).
    /// </summary>
    [Flags]
    public enum InterfaceFlags : ushort
    {
        MultipurposeSlotsMask = 0x001F,
        HasPerInstInfo = 0x0001,
        HasInterfaceMap = 0x0002,
        HasDispatchMapSlot = 0x0004,
        HasNonVirtualSlots = 0x0008,
        HasModuleOverride = 0x0010,
        HasModuleDependencies = 0x0080,
        IsIntrinsicType = 0x0100,
        RequiresDispatchTokenFat = 0x0200,
        HasCctor = 0x0400,
        /// <summary>Type has virtual static methods on interfaces (.NET 8, replaces the old CCW flag).</summary>
        HasVirtualStaticMethods = 0x0800,
        RequiresAlign8 = 0x1000,
        HasBoxedRegularStatics = 0x2000,
        HasSingleNonVirtualSlot = 0x4000,
        DependsOnEquivalentOrForwardedStructs = 0x8000
    }

    /// <summary>
    ///     Low 16 bits of the native m_dwFlags field when component size is not in use
    ///     (.NET 8 WFLAGS_LOW_ENUM). Overlaps <see cref="MethodTable.ComponentSize"/> for
    ///     string/array types.
    /// </summary>
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
        HasVariance = 0x00000100,
        HasDefaultCtor = 0x00000200,
        HasPreciseInitCctors = 0x00000400,
        /// <summary>UNIX_AMD64_ABI only: type is passed in registers by the System V ABI.</summary>
        IsRegStructPassed = 0x00000800,
        IsByRefLike = 0x00001000
    }

    /// <summary>
    ///     Read-only view over the first 64 bytes (fixed header) of a native CoreCLR
    ///     MethodTable. Valid only on configurations accepted by
    ///     <see cref="RuntimeSupport"/>, pointers are views into runtime-owned memory and
    ///     the owning Type must be kept alive for as long as a MethodTable* is used.
    /// </summary>
    [UseAsPointer]
    [Intrinsic]
    [StructLayout(LayoutKind.Explicit)]
    public unsafe partial struct MethodTable
    {
        /// <summary>
        ///     Gets the MethodTable of the provided type.
        /// </summary>
        /// <exception cref="ArgumentNullException">type is null.</exception>
        /// <exception cref="NotSupportedException">
        ///     The type is a pointer, byref, function pointer, generic parameter, or a
        ///     non-runtime (e.g. TypeBuilder) type, these are backed by TypeDescs or have no
        ///     MethodTable.
        /// </exception>
        /// <exception cref="PlatformNotSupportedException">Unsupported runtime/OS/architecture.</exception>
        internal static MethodTable* FromType(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);
            RuntimeSupport.EnsureSupported();

            if (type.IsPointer || type.IsByRef || type.IsFunctionPointer || type.IsGenericParameter)
                throw new NotSupportedException($"{type} is a TypeDesc-backed type and has no MethodTable.");

            // Only genuine runtime types have a MethodTable, TypeBuilder and friends do not.
            if (!typeof(object).GetType().IsInstanceOfType(type))
                throw new NotSupportedException($"{type} is not a runtime type.");

            return (MethodTable*)type.TypeHandle.Value;
        }

        /// <summary>
        ///     Gets the MethodTable of the provided object's type by reading the object header.
        ///     Read-only: performs no metadata writes and never enumerates the object.
        /// </summary>
        /// <exception cref="ArgumentNullException">obj is null.</exception>
        internal static MethodTable* FromObject(object obj)
        {
            ArgumentNullException.ThrowIfNull(obj);
            RuntimeSupport.EnsureSupported();

            // The MethodTable pointer is the first word of every managed object, the
            // heap data reference points one pointer slot past it.
            return *(MethodTable**)Unsafe.Sub(Unsafe.AsPointer(ref Intrinsics.GetData(obj)), sizeof(nint));
        }

        /// <summary>
        ///     Low word of m_dwFlags: generics/statics flags, overlapped by component size
        ///     for array/string types.
        /// </summary>
        [FieldOffset(0)]
        public readonly GenericsFlags GenericsFlags;

        /// <summary>
        ///     m_dwFlags: low 16 bits are the generics flags / component size union, high 16
        ///     bits are the category and type flags.
        /// </summary>
        [FieldOffset(0)]
        public readonly TypeFlags TypeFlags;

        /// <summary>
        ///     Size of the base type represented by the MethodTable.
        /// </summary>
        [FieldOffset(4)]
        public readonly int BaseSize;

        /// <summary>
        ///     m_wFlags2: interface/multipurpose-slot related information.
        /// </summary>
        [FieldOffset(8)]
        public readonly InterfaceFlags InterfaceFlags;

        /// <summary>
        ///     Metadata token associated with the MethodTable.
        /// </summary>
        [FieldOffset(10)]
        public readonly ushort MetadataToken;

        /// <summary>
        ///     Number of virtual methods declared in the MethodTable.
        /// </summary>
        [FieldOffset(12)]
        public readonly ushort NumVirtuals;

        /// <summary>
        ///     Number of interfaces implemented by the type represented by the MethodTable.
        /// </summary>
        [FieldOffset(14)]
        public readonly ushort NumInterfaces;

        /// <summary>
        ///     Pointer to the parent MethodTable from which this MethodTable inherits, if any.
        /// </summary>
        [CanBeNull]
        [FieldOffset(16)]
        public readonly MethodTable* ParentMethodTable;

        /// <summary>
        ///     Pointer to the module that loaded the type associated with the MethodTable.
        /// </summary>
        [CanBeNull]
        [FieldOffset(24)]
        public readonly void* LoaderModule;

        /// <summary>
        ///     Pointer to writeable data associated with the MethodTable.
        /// </summary>
        [FieldOffset(32)]
        public readonly WriteableData* WriteableData;

        /// <summary>
        ///     Part of union 0, pointer to the EEClass (cold data) shared with this MethodTable, if any.
        /// </summary>
        [CanBeNull]
        [FieldOffset(40)]
        public readonly EEClass* EEClass;

        /// <summary>
        ///     Part of union 0, pointer to the canonical MethodTable that represents shared type information, if any.
        ///     The pointer is tagged with the low bit when it is a canonical MethodTable.
        /// </summary>
        [CanBeNull]
        [FieldOffset(40)]
        public readonly MethodTable* CanonMethodTable;

        /// <summary>
        ///     Part of union 1, instance information used by the Runtime for this MethodTable.
        /// </summary>
        [CanBeNull]
        [FieldOffset(48)]
        public readonly byte* PerInstInfo;

        /// <summary>
        ///     Part of union 1, MethodTable to the element type owned by this MethodTable.
        /// </summary>
        [CanBeNull]
        [FieldOffset(48)]
        public readonly MethodTable* ElementMethodTable;

        /// <summary>
        ///     Part of union 1, multipurpose slot 1 information used by the runtime.
        /// </summary>
        [CanBeNull]
        [FieldOffset(48)]
        public readonly byte* MultiPurposeSlot1;

        /// <summary>
        ///     Part of union 2, pointer to the interface map of this MethodTable.
        /// </summary>
        [CanBeNull]
        [FieldOffset(56)]
        public readonly MethodTable* InterfaceMap;

        /// <summary>
        ///     Part of union 2, multipurpose slot 2 information used by the runtime.
        /// </summary>
        [CanBeNull]
        [FieldOffset(56)]
        public readonly byte* MultiPurposeSlot2;

        private const nuint UNION_MASK = 1;

        private nuint UnionRaw => (nuint)CanonMethodTable;

        /// <summary>
        ///     Raw union 0 value masked to the low tag bit.
        /// </summary>
        public Union0Type Union0Type
        {
            get
            {
                return (Union0Type)(UnionRaw & UNION_MASK);
            }
        }

        /// <summary>
        ///     Checks if the union holds an EEClass pointer (canonical MethodTable).
        /// </summary>
        public bool HasEEClass
        {
            get
            {
                return (UnionRaw & UNION_MASK) == (nuint)Union0Type.EEClass;
            }
        }

        /// <summary>
        ///     Checks if the union holds a tagged canonical-MethodTable pointer.
        /// </summary>
        public bool HasCanon
        {
            get
            {
                return (UnionRaw & UNION_MASK) == (nuint)Union0Type.MethodTable;
            }
        }

        /// <summary>
        ///     The canonical (unshared) MethodTable for this instantiation: this MethodTable
        ///     itself when the union holds an EEClass, otherwise the untagged target.
        /// </summary>
        public MethodTable* GetCanonicalMethodTable()
        {
            if (HasCanon)
                return (MethodTable*)(UnionRaw & ~UNION_MASK);

            fixed (MethodTable* ptr = &this)
            {
                return ptr;
            }
        }

        /// <summary>
        ///     Gets the component size of the MethodTable (array element or string char size).
        ///     Only valid while <see cref="TypeFlags.HasComponentSize"/> is set.
        /// </summary>
        public ushort ComponentSize
        {
            get
            {
                fixed (MethodTable* ptr = &this)
                {
                    return HasComponentSize ? *(ushort*)ptr : (ushort)0;
                }
            }
        }

        /// <summary>
        ///     Gets the name of the type associated with this MethodTable.
        /// </summary>
        public string Name
        {
            get
            {
                return AsType().Name;
            }
        }

        /// <summary>
        ///     Always false: a valid MethodTable is never a pointer-type descriptor
        ///     (pointer types are TypeDescs and are rejected by <see cref="FromType"/>).
        /// </summary>
        [Obsolete("A MethodTable is never a pointer type; TypeDesc-backed types are rejected by FromType.")]
        public bool IsPointer => false;

        /// <summary>
        ///     Always false: no library-managed ArrayDesc exists in the native header.
        /// </summary>
        [Obsolete("Cassowary no longer annotates MethodTables with a synthetic ArrayDesc.")]
        public bool HasArrayDesc => false;

        /// <summary>
        ///     Gets a value determining whether or not the associated type has any parent type.
        /// </summary>
        public bool HasParent
        {
            get
            {
                return (nint)ParentMethodTable != 0;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type is an enum type.
        /// </summary>
        public bool IsEnum
        {
            get
            {
                fixed (MethodTable* ptr = &this)
                {
                    return HasParent && ParentMethodTable == FromType(typeof(Enum));
                }
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type is void.
        /// </summary>
        public bool IsVoid
        {
            get
            {
                fixed (MethodTable* ptr = &this)
                {
                    return ptr == FromType(typeof(void));
                }
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type is a string.
        /// </summary>
        public bool IsString
        {
            get
            {
                fixed (MethodTable* ptr = &this)
                {
                    return ptr == FromType(typeof(string));
                }
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type is a string or array type.
        /// </summary>
        public bool IsStringOrArray
        {
            get
            {
                return IsString || IsArray;
            }
        }

        /// <summary>Always throws: the marshal-by-ref category was removed from CoreCLR.</summary>
        [Obsolete("The MarshalByRef category does not exist in .NET 8 CoreCLR MethodTable flags.")]
        public bool IsMarshalByRef => throw new PlatformNotSupportedException();

        /// <summary>Always throws: the contextful category was removed from CoreCLR.</summary>
        [Obsolete("The Contextful category does not exist in .NET 8 CoreCLR MethodTable flags.")]
        public bool IsContextful => throw new PlatformNotSupportedException();

        /// <summary>
        ///     Gets a value determining whether or not the associated type is a value type.
        /// </summary>
        public bool IsValueType
        {
            get
            {
                return (TypeFlags & TypeFlags.ValueTypeMask) == TypeFlags.ValueType;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type is nullable.
        /// </summary>
        public bool IsNullable
        {
            get
            {
                return (TypeFlags & TypeFlags.Mask) == TypeFlags.Nullable;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type is a primitive value type (enum, primitive, etc.)
        /// </summary>
        public bool IsPrimitiveValueType
        {
            get
            {
                return (TypeFlags & TypeFlags.Mask) == TypeFlags.PrimitiveValueType;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type is a true primitive type.
        /// </summary>
        public bool IsTruePrimitive
        {
            get
            {
                return (TypeFlags & TypeFlags.Mask) == TypeFlags.TruePrimitive;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type is an Array.
        /// </summary>
        public bool IsArray
        {
            get
            {
                return (TypeFlags & TypeFlags.ArrayMask) == TypeFlags.Array;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type is an SZArray.
        /// </summary>
        public bool IsSZArray
        {
            get
            {
                return IsArray && (TypeFlags & TypeFlags.IfArrayThenSzArray) != 0;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type is an interface.
        /// </summary>
        public bool IsInterface
        {
            get
            {
                return (TypeFlags & TypeFlags.Mask) == TypeFlags.Interface;
            }
        }

        /// <summary>Always throws: there is no GuidInfo flag in .NET 8 CoreCLR MethodTable flags.</summary>
        [Obsolete("The HasGuidInfo interface flag does not exist in .NET 8 CoreCLR MethodTable flags.")]
        public bool IsInterfaceWithGuidInfo => throw new PlatformNotSupportedException();

        /// <summary>Always throws: the transparent-proxy category was removed from CoreCLR.</summary>
        [Obsolete("The TransparentProxy category does not exist in .NET 8 CoreCLR MethodTable flags.")]
        public bool IsTransparentProxy => throw new PlatformNotSupportedException();

        /// <summary>Always throws: the async-pin category was removed from CoreCLR.</summary>
        [Obsolete("The AsyncPin category does not exist in .NET 8 CoreCLR MethodTable flags.")]
        public bool IsAsyncPin => throw new PlatformNotSupportedException();

        /// <summary>
        ///     Gets a value determining whether or not the associated type has a finalizer.
        /// </summary>
        public bool HasFinalizer
        {
            get
            {
                return (TypeFlags & TypeFlags.HasFinalizer) != 0;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type is an ICastable.
        /// </summary>
        public bool IsICastable
        {
            get
            {
                return (TypeFlags & TypeFlags.ICastable) != 0;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type implements
        ///     IDynamicInterfaceCastable.
        /// </summary>
        public bool IsIDynamicInterfaceCastable
        {
            get
            {
                return (TypeFlags & TypeFlags.IDynamicInterfaceCastable) != 0;
            }
        }

        /// <summary>Always throws: the indirect-parent meaning was removed from .NET 8 flags.</summary>
        [Obsolete("The HasIndirectParent bit is unused in .NET 8 CoreCLR MethodTable flags.")]
        public bool HasIndirectParent => throw new PlatformNotSupportedException();

        /// <summary>
        ///     Gets a value determining whether or not the associated type contains any pointers.
        /// </summary>
        public bool ContainsPointers
        {
            get
            {
                return (TypeFlags & TypeFlags.ContainsPointers) != 0;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type is equivalent to any other types.
        /// </summary>
        public bool HasTypeEquivalence
        {
            get
            {
                return (TypeFlags & TypeFlags.HasTypeEquivalence) != 0;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type is a tracked
        ///     reference with a finalizer.
        /// </summary>
        public bool IsTrackedReferenceWithFinalizer
        {
            get
            {
                return (TypeFlags & TypeFlags.IsTrackedReferenceWithFinalizer) != 0;
            }
        }

        /// <summary>Always throws: the RCW per-type-data bit was removed from .NET 8 flags.</summary>
        [Obsolete("The HasRCWPerTypeData bit does not exist in .NET 8 CoreCLR MethodTable flags.")]
        public bool HasRCWPerTypeData => throw new PlatformNotSupportedException();

        /// <summary>
        ///     Gets a value determining whether or not the associated type has a critical finalizer that must be executed.
        /// </summary>
        public bool HasCriticalFinalizer
        {
            get
            {
                return (TypeFlags & TypeFlags.HasCriticalFinalizer) != 0;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type is collectible.
        /// </summary>
        public bool IsCollectible
        {
            get
            {
                return (TypeFlags & TypeFlags.Collectible) != 0;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type contains any generic variables.
        /// </summary>
        public bool ContainsGenericVariables
        {
            get
            {
                return (TypeFlags & TypeFlags.ContainsGenericVariables) != 0;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type is a COM object.
        /// </summary>
        public bool IsCOMObject
        {
            get
            {
                return (TypeFlags & TypeFlags.ComObject) != 0;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type has a component size, like an Array.
        /// </summary>
        public bool HasComponentSize
        {
            get
            {
                return (TypeFlags & TypeFlags.HasComponentSize) != 0;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type has a non-trivial interface cast.
        /// </summary>
        public bool HasNonTrivialInterfaceCast
        {
            get
            {
                return (TypeFlags & TypeFlags.NonTrivialInterfaceCast) != 0;
            }
        }

        /// <summary>
        ///     The generics flags word when component size is not in use. For arrays and
        ///     strings the low word is component size, so the effective flags are the
        ///     native default StringArrayValues, which is 0.
        /// </summary>
        private GenericsFlags EffectiveLowFlags => HasComponentSize ? (GenericsFlags)0 : GenericsFlags;

        /// <summary>
        ///     Gets a value determining whether or not the associated type is a static dynamic.
        /// </summary>
        public bool IsDynamicStatics
        {
            get
            {
                return (EffectiveLowFlags & GenericsFlags.StaticsMask) != GenericsFlags.StaticsMask_NonDynamic;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type is not in it's preferred zap module.
        /// </summary>
        public bool IsNotInPZM
        {
            get
            {
                return (EffectiveLowFlags & GenericsFlags.NotInPZM) != 0;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type is a generic instance.
        /// </summary>
        public bool IsGenericInst
        {
            get
            {
                return (EffectiveLowFlags & GenericsFlags.GenericsMask) == GenericsFlags.GenericsMask_GenericInst;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type is a shared instance.
        /// </summary>
        public bool IsGenericSharedInst
        {
            get
            {
                return (EffectiveLowFlags & GenericsFlags.GenericsMask) == GenericsFlags.GenericsMask_SharedInst;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type is a typical instance.
        /// </summary>
        public bool IsGenericTypicalInst
        {
            get
            {
                return (EffectiveLowFlags & GenericsFlags.GenericsMask) == GenericsFlags.GenericsMask_TypicalInst;
            }
        }

        /// <summary>Always false: remoting VTS state was removed from CoreCLR.</summary>
        [Obsolete("Remoting VTS state does not exist in .NET 8 CoreCLR.")]
        public bool HasRemotingVtsInfo => false;

        /// <summary>
        ///     Gets a value determining whether or not the associated type has variance.
        /// </summary>
        public bool HasVariance
        {
            get
            {
                return (EffectiveLowFlags & GenericsFlags.HasVariance) != 0;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type has a default constuctor.
        /// </summary>
        public bool HasDefaultCtor
        {
            get
            {
                return (EffectiveLowFlags & GenericsFlags.HasDefaultCtor) != 0;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type has precise init constructors.
        /// </summary>
        public bool HasPreciseInitCtors
        {
            get
            {
                return (EffectiveLowFlags & GenericsFlags.HasPreciseInitCctors) != 0;
            }
        }

        /// <summary>
        ///     Always false on the supported x64 ABIs, HFA is an ARM-only classification.
        /// </summary>
        public bool IsHFA => false;

        /// <summary>
        ///     Gets a value determining whether or not the associated struct type is passed
        ///     in registers by the System V ABI. Only meaningful on Linux x64.
        /// </summary>
        public bool IsRegStructPassed
        {
            get
            {
                return RuntimeSupport.IsLinux && (EffectiveLowFlags & GenericsFlags.IsRegStructPassed) != 0;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type is passed like a by-ref.
        /// </summary>
        public bool IsByRefLike
        {
            get
            {
                return (EffectiveLowFlags & GenericsFlags.IsByRefLike) != 0;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type has any per-instance information.
        /// </summary>
        public bool HasPerInstInfo
        {
            get
            {
                return (InterfaceFlags & InterfaceFlags.HasPerInstInfo) != 0;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type has an InterfaceMap, the map length == NumInterfaces.
        /// </summary>
        public bool HasInterfaceMap
        {
            get
            {
                return (InterfaceFlags & InterfaceFlags.HasInterfaceMap) != 0;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type has a dispatch map slot.
        /// </summary>
        public bool HasDispatchMapSlot
        {
            get
            {
                return (InterfaceFlags & InterfaceFlags.HasDispatchMapSlot) != 0;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type has any non-virtual slots.
        /// </summary>
        public bool HasNonVirtualSlots
        {
            get
            {
                return (InterfaceFlags & InterfaceFlags.HasNonVirtualSlots) != 0;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type has module overrides.
        /// </summary>
        public bool HasModuleOverride
        {
            get
            {
                return (InterfaceFlags & InterfaceFlags.HasModuleOverride) != 0;
            }
        }

        /// <summary>Always false: pre-restored/zapped state does not exist in .NET 8 flags2.</summary>
        [Obsolete("The IsZapped/IsPreRestored flags do not exist in .NET 8 CoreCLR MethodTable flags2.")]
        public bool IsZapped => false;

        /// <summary>Always false: pre-restored/zapped state does not exist in .NET 8 flags2.</summary>
        [Obsolete("The IsZapped/IsPreRestored flags do not exist in .NET 8 CoreCLR MethodTable flags2.")]
        public bool IsPreRestored => false;

        /// <summary>
        ///     Gets a value determining whether or not the associated type depends on an external module.
        /// </summary>
        public bool HasModuleDependencies
        {
            get
            {
                return (InterfaceFlags & InterfaceFlags.HasModuleDependencies) != 0;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type is intrinsic.
        /// </summary>
        public bool IsIntrinsicType
        {
            get
            {
                return (InterfaceFlags & InterfaceFlags.IsIntrinsicType) != 0;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type requires a fat dispatch token.
        /// </summary>
        public bool RequiresDispatchTokenFat
        {
            get
            {
                return (InterfaceFlags & InterfaceFlags.RequiresDispatchTokenFat) != 0;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type has any constructor.
        /// </summary>
        public bool HasCctor
        {
            get
            {
                return (InterfaceFlags & InterfaceFlags.HasCctor) != 0;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type has virtual static
        ///     methods (e.g. on interfaces). Replaces the removed COM CCW-template flag.
        /// </summary>
        public bool HasVirtualStaticMethods
        {
            get
            {
                return (InterfaceFlags & InterfaceFlags.HasVirtualStaticMethods) != 0;
            }
        }

        /// <summary>Always throws: CCW templates do not exist without FEATURE_COMINTEROP.</summary>
        [Obsolete("CCW templates are COM-only and do not exist in this MethodTable flag on .NET 8.")]
        public bool HasCCWTemplate => throw new PlatformNotSupportedException();

        /// <summary>
        ///     Gets a value determining whether or not the associated type requires alignment of 8 bytes in memory.
        /// </summary>
        public bool RequiresAlign8
        {
            get
            {
                return (InterfaceFlags & InterfaceFlags.RequiresAlign8) != 0;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type has boxed regular statics.
        /// </summary>
        public bool HasBoxedRegularStatics
        {
            get
            {
                return (InterfaceFlags & InterfaceFlags.HasBoxedRegularStatics) != 0;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type only has one non-virtual slot available.
        /// </summary>
        public bool HasSingleNonVirtualSlot
        {
            get
            {
                return (InterfaceFlags & InterfaceFlags.HasSingleNonVirtualSlot) != 0;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type depends on equivalent or forwarded structs.
        /// </summary>
        public bool DependsOnEquivalentOrForwardedStructs
        {
            get
            {
                return (InterfaceFlags & InterfaceFlags.DependsOnEquivalentOrForwardedStructs) != 0;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type is initialized before ever having been accessed.
        /// </summary>
        public bool IsClassPreInited
        {
            get
            {
                return ContainsGenericVariables || (!HasCctor && !HasBoxedRegularStatics && !IsDynamicStatics);
            }
        }

        /// <summary>
        ///     Number of instance field bytes (BaseSize minus the EEClass padding), the same
        ///     value native code uses for non-array object sizes.
        /// </summary>
        public int GetNumInstanceFieldBytes()
        {
            return BaseSize - GetClass()->BaseSizePadding;
        }

        /// <summary>
        ///     Gets the EEClass of this MethodTable, following the canonical MethodTable when
        ///     this is a shared generic instantiation (union tag removed before use).
        /// </summary>
        /// <returns>The EEClass of this MethodTable.</returns>
        public EEClass* GetClass()
        {
            return GetCanonicalMethodTable()->EEClass;
        }

        /// <summary>
        ///     Gets the root MethodTable from the Canonical MethodTable.
        /// </summary>
        /// <returns>Root MethodTable.</returns>
        public MethodTable* GetRootCanonTable()
        {
            fixed (MethodTable* ptr = &this)
            {
                MethodTable* mt = ptr;
                while (mt->HasCanon)
                    mt = (MethodTable*)(mt->UnionRaw & ~UNION_MASK);

                return mt;
            }
        }

        /// <summary>
        ///     Gets the type of this MethodTable.
        /// </summary>
        /// <returns>The type of this MethodTable.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Type AsType()
        {
            RuntimeSupport.EnsureSupported();

            fixed (MethodTable* ptr = &this)
            {
                return Type.GetTypeFromHandle(RuntimeTypeHandle.FromIntPtr((nint)ptr)!)!;
            }
        }

        /// <summary>
        ///     Gets all interfaces that this MethodTable has.
        /// </summary>
        /// <returns>All interfaces that this MethodTable has.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MethodTable*[] GetInterfaces()
        {
            if (!HasInterfaceMap || InterfaceMap == null)
                return new MethodTable*[0];

            MethodTable*[] interfaces = new MethodTable*[NumInterfaces];

            // m_pInterfaceMap is an array of InterfaceInfo_t, each of which starts with a
            // single MethodTable pointer, load the pointer with pointer-size stride.
            MethodTable** entries = (MethodTable**)InterfaceMap;
            for (int i = 0; i < NumInterfaces; i++)
                interfaces[i] = entries[i];

            return interfaces;
        }

        /// <summary>
        ///     Checks if this MethodTable is equivalent to the given MethodTable.
        /// </summary>
        /// <param name="pMT">The MethodTable to check if equivalent to.</param>
        /// <returns>True if this MethodTable is equivalent to the given MethodTable, otherwise false.</returns>
        public bool IsEquivalentTo(MethodTable* pMT)
        {
            return AsType().IsEquivalentTo(pMT->AsType());
        }

        /// <summary>
        ///     Checks if this MethodTable has any interface implementations in common with the given MethodTable.
        /// </summary>
        /// <param name="pMT">The MethodTable to check for commonalities with.</param>
        /// <returns>True if this MethodTable has any common interfaces, otherwise false.</returns>
        public bool ImplementsCommonInterface(MethodTable* pMT)
        {
            Type[] ours = AsType().GetInterfaces();
            Type[] theirs = pMT->AsType().GetInterfaces();

            foreach (Type o in ours)
                foreach (Type t in theirs)
                    if (o == t)
                        return true;

            return false;
        }

        /// <summary>
        ///     Checks if this MethodTable implements the given interface.
        /// </summary>
        /// <param name="pMT">The MethodTable to check if implemented.</param>
        /// <returns>True if this MethodTable implements the given interface, otherwise false.</returns>
        public bool ImplementsInterface(MethodTable* pMT)
        {
            return pMT->AsType().IsInterface && pMT->AsType().IsAssignableFrom(AsType());
        }

        /// <summary>
        ///     Checks if this MethodTable implements any interface equivalent to the given interface.
        /// </summary>
        /// <param name="pMT">The MethodTable to check if implements an equivalent to.</param>
        /// <returns>True if this MethodTable implements an equivalent to the given interface, otherwise false.</returns>
        public bool ImplementsEquivalentInterface(MethodTable* pMT)
        {
            foreach (Type implemented in AsType().GetInterfaces())
            {
                if (implemented.IsEquivalentTo(pMT->AsType()))
                    return true;
            }

            return false;
        }

        /// <summary>
        ///     Checks if this MethodTable can allocate an object.
        /// </summary>
        /// <remarks>
        ///     Mirrors the managed allocation guards used by <see cref="AllocateNoChecks"/>,
        ///     see the allocation methods for the concrete reject set.
        /// </remarks>
        public bool CanAllocate()
        {
            return IsAllocatableType(AsType());
        }

        internal static bool IsAllocatableType(Type type)
        {
            // Reject every category that supported managed allocation cannot produce.
            if (type.ContainsGenericParameters
                || type.IsByRefLike
                || type.IsInterface
                || type.IsAbstract
                || type.IsPointer
                || type.IsByRef
                || type.IsFunctionPointer
                || type.IsGenericParameter
                || type == typeof(void)
                || type == typeof(TypedReference)
                || type == typeof(RuntimeArgumentHandle)
                || type == typeof(ArgIterator)
                || typeof(Delegate).IsAssignableFrom(type))
                return false;

            if (type.IsArray)
                return !type.GetElementType()!.ContainsGenericParameters;

            if (type == typeof(string))
                return true;

            return type.IsValueType || type.IsClass;
        }

        /// <summary>
        ///     Checks if this MethodTable can box from a pointer.
        /// </summary>
        public bool CanBox()
        {
            return IsBoxableType(AsType());
        }

        /// <summary>
        ///     Checks if this MethodTable can box strictly from a pointer.
        /// </summary>
        public bool CanBoxStrict()
        {
            return IsBoxableType(AsType());
        }

        internal static bool IsBoxableType(Type type)
        {
            // Only closed, non-byref-like value types without GC references can be safely
            // copied from raw storage. Nullable<T> is included (boxing handles empty/populated).
            return type.IsValueType
                && type != typeof(void)
                && !type.ContainsGenericParameters
                && !type.IsByRefLike
                && !ContainsReferencesHelper(type);
        }

        private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, bool> s_containsRefs = new();

        internal static bool ContainsReferencesHelper(Type type)
        {
            return s_containsRefs.GetOrAdd(type,
                t => (bool)s_isReferenceOrContainsReferences.MakeGenericMethod(t).Invoke(null, null)!);
        }

        private static readonly MethodInfo s_isReferenceOrContainsReferences =
            typeof(MethodTable).GetMethod(nameof(IsReferenceOrContainsReferences),
                BindingFlags.NonPublic | BindingFlags.Static)!;

        private static bool IsReferenceOrContainsReferences<T>() => RuntimeHelpers.IsReferenceOrContainsReferences<T>();

        /// <summary>
        ///     Checks if this MethodTable can cast an object to the given Interface MethodTable.
        /// </summary>
        public bool CanCastToInterface(MethodTable* pMT)
        {
            return pMT->AsType().IsInterface && pMT->AsType().IsAssignableFrom(AsType());
        }

        /// <summary>
        ///     Checks if this MethodTable can cast an object to the given MethodTable.
        ///     Delegates to the runtime's own assignability rules (covers inheritance,
        ///     interfaces, variance, and array covariance).
        /// </summary>
        public bool CanCastTo(MethodTable* pMT)
        {
            return pMT->AsType().IsAssignableFrom(AsType());
        }

        /// <summary>
        ///     Initializes the type associated with this MethodTable if it has not already.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Initialize()
        {
            RuntimeHelpers.RunClassConstructor(AsType().TypeHandle);
        }

        /// <summary>
        ///     Constructs on the given object of this MethodTable with the given parameters.
        /// </summary>
        /// <param name="instance">The object to be constructed.</param>
        /// <param name="parameters">The parameters for the object's constructor.</param>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public void Construct<T>(ref T instance, params object[] parameters)
        {
            ConstructorInfo ctorInfo = FindConstructor(parameters);
            new Signature(ctorInfo).Invoke(ref instance, parameters);
        }

        /// <summary>
        ///     Constructs this MethodTable with the given parameters.
        /// </summary>
        /// <param name="parameters">The parameters for the object's constructor.</param>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public object Construct(params object[] parameters)
        {
            ConstructorInfo ctorInfo = FindConstructor(parameters);
            return new Signature(ctorInfo).Invoke(parameters);
        }

        private ConstructorInfo FindConstructor(object[] parameters)
        {
            Type[] parameterTypes = new Type[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
                parameterTypes[i] = parameters[i].GetType();

            return AsType().GetConstructor(
                       BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                       parameterTypes)!;
        }

        public override string ToString()
        {
            return Name ?? string.Empty;
        }
    }
}
