using Cassowary.Attributes;
using Cassowary.Runtime.Cor;
using JetBrains.Annotations;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Cassowary.Runtime.VM.EE
{
    /// <summary>
    ///     .NET 8 CoreCLR EEClass VM flags (class.h VMFLAG_*).
    /// </summary>
    [Flags]
    public enum VMFlags : uint
    {
        LayoutDependsOnOtherModules = 0x00000001,
        Delegate = 0x00000002,
        EncStaticFields = 0x00000004,
        FixedAddressVtStatics = 0x00000020,
        HasLayout = 0x00000040,
        IsNested = 0x00000080,
        IsEquivalentType = 0x00000200,
        HasOverlayedFields = 0x00000400,
        HasFieldsWhichMustBeInited = 0x00000800,
        UnsafeValueType = 0x00001000,
        BestFitMappingInited = 0x00002000,
        BestFitMapping = 0x00004000,
        ThrowOnUnmappableChar = 0x00008000,
        InlineArray = 0x00010000,
        NoGuid = 0x00020000,
        HasNonPublicFields = 0x00040000,
        HasCustomFieldAlignment = 0x00080000,
        ContainsStackPtr = 0x00100000,
        PreferAlign8 = 0x00200000,
        OnlyAbstractMethods = 0x00400000,
        SparseForCominterop = 0x00800000,
        HasCoClassAttrib = 0x01000000,
        ComEventItfMask = 0x02000000,
        /// <summary>Class uses MethodImpl to override a virtual function defined on a class.</summary>
        VTableMethodImpl = 0x04000000,
        /// <summary>Class has a covariant override.</summary>
        CovariantOverride = 0x08000000,
        NotTightlyPacked = 0x10000000,
        ContainsMethodImpls = 0x20000000,
        MarshalingTypeMask = 0xc0000000,
        MarshalingTypeInhibit = 0x40000000,
        MarshalingTypeFreeThreaded = 0x80000000,
        MarshalingTypeStandard = 0xc0000000
    }

    /// <summary>
    ///     Read-only view of a native CoreCLR EEClass. The first five pointer fields are at
    ///     fixed offsets on every supported configuration, the trailing attribute/flag/byte
    ///     fields are platform-dependent because FEATURE_COMINTEROP fields exist only on
    ///     Windows.
    /// </summary>
    /// <remarks>
    ///     Windows x64: COM fields at 40/48, attributes at 56, flags at 60, byte fields at
    ///     64-67 (base size 72). Linux x64: no COM fields, attributes at 40, flags at 44,
    ///     byte fields at 48-51 (base size 56). Derived classes (ArrayClass,
    ///     DelegateEEClass, LayoutEEClass) begin immediately after the base.
    /// </remarks>
    [UseAsPointer]
    [Intrinsic]
    [StructLayout(LayoutKind.Explicit)]
    public readonly unsafe struct EEClass
    {
        /// <summary>Size of the base EEClass fields on the current platform.</summary>
        internal static int BaseSize
        {
            get
            {
                RuntimeSupport.EnsureSupported();
                return RuntimeSupport.IsWindows ? 72 : 56;
            }
        }

        private static int AttrsOffset => RuntimeSupport.IsWindows ? 56 : 40;
        private static int VMFlagsOffset => RuntimeSupport.IsWindows ? 60 : 44;
        private static int NormTypeOffset => RuntimeSupport.IsWindows ? 64 : 48;
        private static int PackedOffset => RuntimeSupport.IsWindows ? 65 : 49;
        private static int FixedFieldsOffset => RuntimeSupport.IsWindows ? 66 : 50;
        private static int PaddingOffset => RuntimeSupport.IsWindows ? 67 : 51;

        /// <summary>
        ///     Contains the GuidInfo if the EEClass is associated with an Interface.
        /// </summary>
        [CanBeNull]
        [FieldOffset(0)]
        public readonly GuidInfo* GuidInfo; // m_pGuidInfo

        /// <summary>
        ///     Contains Optional Fields if specified by the associated type.
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

        private byte* Addr(int offset)
        {
            RuntimeSupport.EnsureSupported();

            fixed (EEClass* ptr = &this)
            {
                return (byte*)ptr + offset;
            }
        }

        /// <summary>
        ///     Handle to a delegate used to allocate an aggregated unmanaged class.
        ///     COM interop only, exists only on Windows.
        /// </summary>
        /// <exception cref="PlatformNotSupportedException">Non-Windows runtime.</exception>
        public void* DelegateObjectHandle
        {
            get
            {
                if (!RuntimeSupport.IsWindows)
                    throw new PlatformNotSupportedException("m_ohDelegate/m_ComInterfaceType exist only under FEATURE_COMINTEROP (Windows).");

                return *(void**)Addr(40);
            }
        }

        /// <summary>
        ///     COM interface type for interfaces. COM interop only, exists only on Windows.
        /// </summary>
        /// <exception cref="PlatformNotSupportedException">Non-Windows runtime.</exception>
        public CorInterfaceType CorInterfaceType
        {
            get
            {
                if (!RuntimeSupport.IsWindows)
                    throw new PlatformNotSupportedException("m_ohDelegate/m_ComInterfaceType exist only under FEATURE_COMINTEROP (Windows).");

                return Unsafe.Read<CorInterfaceType>(Addr(40));
            }
        }

        /// <summary>
        ///     Contains the ComCallWrapperTemplate if the EEClass is associated with a COM
        ///     visible type. COM interop only, exists only on Windows.
        /// </summary>
        /// <exception cref="PlatformNotSupportedException">Non-Windows runtime.</exception>
        public CCWTemplate* CCWTemplate
        {
            get
            {
                if (!RuntimeSupport.IsWindows)
                    throw new PlatformNotSupportedException("m_pccwTemplate exists only under FEATURE_COMINTEROP (Windows).");

                return *(CCWTemplate**)Addr(48);
            }
        }

        /// <summary>TypeAttributes (m_dwAttrClass).</summary>
        public TypeAttributes TypeAttributes => Unsafe.Read<TypeAttributes>(Addr(AttrsOffset));

        /// <summary>VM flags (m_VMFlags).</summary>
        public VMFlags VMFlags => Unsafe.Read<VMFlags>(Addr(VMFlagsOffset));

        /// <summary>Normalized element type (m_NormType).</summary>
        public CorElementType CorElementType => Unsafe.Read<CorElementType>(Addr(NormTypeOffset));

        /// <summary>TRUE iff the fields are in packed state (m_fFieldsArePacked).</summary>
        public bool FieldsArePacked => Unsafe.Read<byte>(Addr(PackedOffset)) != 0;

        /// <summary>Count of bytes of normal fields of this instance (m_cbFixedEEClassFields).</summary>
        public byte FixedEEClassFieldsSize => Unsafe.Read<byte>(Addr(FixedFieldsOffset));

        /// <summary>Bytes of padding included in the MethodTable BaseSize (m_cbBaseSizePadding).</summary>
        public byte BaseSizePadding => Unsafe.Read<byte>(Addr(PaddingOffset));

        /// <summary>
        ///     Gets the rank of the array.
        /// </summary>
        /// <remarks>
        ///     If the type is not an array, the rank will be 0.
        /// </remarks>
        public int Rank
        {
            get
            {
                return HasArrayClass ? AsArrayClass()->Rank : 0;
            }
        }

        /// <summary>
        ///     Managed (GC) instance size.
        /// </summary>
        /// <remarks>
        ///     For types with layout information this is the layout managed size, otherwise
        ///     it is the MethodTable instance-field byte count. Array and string sizes are
        ///     instance-dependent and cannot be answered from the EEClass.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        ///     The type is an array or string, use an actual instance for its size.
        /// </exception>
        public int ManagedSize
        {
            get
            {
                Type type = MethodTable->AsType();

                if (type.IsArray || type == typeof(string))
                    throw new InvalidOperationException(
                        "Managed size of arrays and strings is instance-dependent. Query an instance instead.");

                if (HasLayout)
                    return LayoutEEClass->LayoutInfo.ManagedSize;

                return MethodTable->GetNumInstanceFieldBytes();
            }
        }

        /// <summary>
        ///     Gets the EEClass viewed as a LayoutEEClass (valid only when
        ///     <see cref="HasLayout"/> is set).
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
        ///     Whether this interface type carries a cached GuidInfo.
        /// </summary>
        public bool HasGuidInfo
        {
            get
            {
                return GuidInfo != null && !NoGuid && MethodTable->IsInterface;
            }
        }

        /// <summary>
        ///     EEClasses only contain Optional Fields if specified so by the associated type.
        /// </summary>
        public bool HasOptionalFields
        {
            get
            {
                return OptionalFields != null;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type has an ArrayClass.
        /// </summary>
        public bool HasArrayClass
        {
            get
            {
                return CorElementType == CorElementType.ELEMENT_TYPE_ARRAY || CorElementType == CorElementType.ELEMENT_TYPE_SZARRAY;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type has a DelegateEEClass.
        /// </summary>
        public bool HasDelegateEEClass
        {
            get
            {
                return IsDelegate;
            }
        }

        /// <summary>
        ///     Gets a value determining whether or not the associated type is a primitive type.
        /// </summary>
        public bool IsPrimitive
        {
            get
            {
                return Intrinsics.CorIsPrimitiveType(CorElementType);
            }
        }

        private int Visibility => (int)(TypeAttributes & TypeAttributes.VisibilityMask);
        private int Layout => (int)(TypeAttributes & TypeAttributes.LayoutMask);
        private int StringFormat => (int)(TypeAttributes & TypeAttributes.StringFormatMask);
        private int ClassSemantics => (int)(TypeAttributes & TypeAttributes.ClassSemanticsMask);

        /// <summary>Uses CharSet.Ansi.</summary>
        public bool IsAnsiClass => StringFormat == (int)TypeAttributes.AnsiClass;

        /// <summary>Uses auto layout.</summary>
        public bool HasAutoLayout => Layout == (int)TypeAttributes.AutoLayout;

        /// <summary>Is an interface.</summary>
        public bool IsInterface => ClassSemantics == (int)TypeAttributes.Interface;

        /// <summary>Is a class.</summary>
        public bool IsClass => ClassSemantics == (int)TypeAttributes.Class;

        /// <summary>Is not public.</summary>
        public bool IsNotPublic => Visibility == (int)TypeAttributes.NotPublic;

        /// <summary>Is public.</summary>
        public bool IsPublic => Visibility == (int)TypeAttributes.Public;

        /// <summary>Is nested public.</summary>
        public bool IsNestedPublic => Visibility == (int)TypeAttributes.NestedPublic;

        /// <summary>Is nested private.</summary>
        public bool IsNestedPrivate => Visibility == (int)TypeAttributes.NestedPrivate;

        /// <summary>Is nested family.</summary>
        public bool IsNestedFamily => Visibility == (int)TypeAttributes.NestedFamily;

        /// <summary>Is nested assembly.</summary>
        public bool IsNestedAssembly => Visibility == (int)TypeAttributes.NestedAssembly;

        /// <summary>Is nested family and assembly.</summary>
        public bool IsNestedFamANDAssem => Visibility == (int)TypeAttributes.NestedFamANDAssem;

        /// <summary>Is nested family or assembly.</summary>
        public bool IsNestedFamORAssem => Visibility == (int)TypeAttributes.NestedFamORAssem;

        /// <summary>Has sequential layout.</summary>
        public bool HasSequentialLayout => Layout == (int)TypeAttributes.SequentialLayout;

        /// <summary>Has explicit layout.</summary>
        public bool HasExplicitLayout => Layout == (int)TypeAttributes.ExplicitLayout;

        /// <summary>Is abstract.</summary>
        public bool IsAbstract => (TypeAttributes & TypeAttributes.Abstract) != 0;

        /// <summary>Is sealed.</summary>
        public bool IsSealed => (TypeAttributes & TypeAttributes.Sealed) != 0;

        /// <summary>Is a special name.</summary>
        public bool IsSpecialName => (TypeAttributes & TypeAttributes.SpecialName) != 0;

        /// <summary>Is a runtime special name.</summary>
        public bool IsRTSpecialName => (TypeAttributes & TypeAttributes.RTSpecialName) != 0;

        /// <summary>Is an import.</summary>
        public bool IsImport => (TypeAttributes & TypeAttributes.Import) != 0;

        /// <summary>Is serializable.</summary>
        public bool IsSerializable => (TypeAttributes & TypeAttributes.Serializable) != 0;

        /// <summary>Is a Windows runtime type.</summary>
        public bool IsWindowsRuntime => (TypeAttributes & TypeAttributes.WindowsRuntime) != 0;

        /// <summary>Uses CharSet.Unicode.</summary>
        public bool IsUnicodeClass => StringFormat == (int)TypeAttributes.UnicodeClass;

        /// <summary>Uses CharSet.Auto.</summary>
        public bool IsAutoClass => StringFormat == (int)TypeAttributes.AutoClass;

        /// <summary>Uses a custom string format.</summary>
        public bool HasCustomFormat => StringFormat == (int)TypeAttributes.CustomFormatClass;

        /// <summary>Has security.</summary>
        public bool HasSecurity => (TypeAttributes & TypeAttributes.HasSecurity) != 0;

        /// <summary>Layout depends on other modules.</summary>
        public bool LayoutDependsOnOtherModules => (VMFlags & VMFlags.LayoutDependsOnOtherModules) != 0;

        /// <summary>Is a delegate.</summary>
        public bool IsDelegate => (VMFlags & VMFlags.Delegate) != 0;

        /// <summary>Value type statics will be pinned.</summary>
        public bool FixedAddressVtStatics => (VMFlags & VMFlags.FixedAddressVtStatics) != 0;

        /// <summary>Has layout information (is a LayoutEEClass).</summary>
        public bool HasLayout => (VMFlags & VMFlags.HasLayout) != 0;

        /// <summary>Is nested.</summary>
        public bool IsNested => (VMFlags & VMFlags.IsNested) != 0;

        /// <summary>Is an equivalent type.</summary>
        public bool IsEquivalentType => (VMFlags & VMFlags.IsEquivalentType) != 0;

        /// <summary>Has overlayed fields.</summary>
        public bool HasOverlayedFields => (VMFlags & VMFlags.HasOverlayedFields) != 0;

        /// <summary>Has fields which must be explicitly initialized.</summary>
        public bool HasFieldsWhichMustBeInited => (VMFlags & VMFlags.HasFieldsWhichMustBeInited) != 0;

        /// <summary>Is an unsafe value type.</summary>
        public bool UnsafeValueType => (VMFlags & VMFlags.UnsafeValueType) != 0;

        /// <summary>BestFitMapping/ThrowOnUnmappableChar values are initialized.</summary>
        public bool BestFitMappingInited => (VMFlags & VMFlags.BestFitMappingInited) != 0;

        /// <summary>BestFitMapping is enabled.</summary>
        public bool BestFitMapping => (VMFlags & VMFlags.BestFitMapping) != 0;

        /// <summary>ThrowOnUnmappableChar is enabled.</summary>
        public bool ThrowOnUnmappableChar => (VMFlags & VMFlags.ThrowOnUnmappableChar) != 0;

        /// <summary>Type is an inline array.</summary>
        public bool IsInlineArray => (VMFlags & VMFlags.InlineArray) != 0;

        /// <summary>Type has no Guid.</summary>
        public bool NoGuid => (VMFlags & VMFlags.NoGuid) != 0;

        /// <summary>Type has non-public fields.</summary>
        public bool HasNonPublicField => (VMFlags & VMFlags.HasNonPublicFields) != 0;

        /// <summary>Type has custom field alignment.</summary>
        public bool HasCustomFieldAlignment => (VMFlags & VMFlags.HasCustomFieldAlignment) != 0;

        /// <summary>Type contains a stack pointer.</summary>
        public bool ContainsStackPtr => (VMFlags & VMFlags.ContainsStackPtr) != 0;

        /// <summary>Type prefers 8-byte alignment.</summary>
        public bool PreferAlign8 => (VMFlags & VMFlags.PreferAlign8) != 0;

        /// <summary>Type only contains abstract methods.</summary>
        public bool OnlyAbstractMethods => (VMFlags & VMFlags.OnlyAbstractMethods) != 0;

        /// <summary>Type is sparse for COM interop.</summary>
        public bool SparseForCominterop => (VMFlags & VMFlags.SparseForCominterop) != 0;

        /// <summary>Type has a CoClass attribute (COM only metadata flag, bit still exists).</summary>
        public bool HasCoClassAttrib => (VMFlags & VMFlags.HasCoClassAttrib) != 0;

        /// <summary>Type is a special COM event interface.</summary>
        public bool IsComEventItf => (VMFlags & VMFlags.ComEventItfMask) != 0;

        /// <summary>Class uses MethodImpl to override a class virtual function.</summary>
        public bool HasVTableMethodImpl => (VMFlags & VMFlags.VTableMethodImpl) != 0;

        /// <summary>Class has a covariant override.</summary>
        public bool HasCovariantOverride => (VMFlags & VMFlags.CovariantOverride) != 0;

        /// <summary>Always throws: the WinRT projection flag was removed from .NET 8 VM flags.</summary>
        [Obsolete("The ProjectedFromWinRT bit is now VMFLAG_VTABLEMETHODIMPL in .NET 8.")]
        public bool IsProjectedFromWinRT => throw new PlatformNotSupportedException();

        /// <summary>Always throws: the WinRT export flag was removed from .NET 8 VM flags.</summary>
        [Obsolete("The ExportedToWinRT bit is now VMFLAG_COVARIANTOVERRIDE in .NET 8.")]
        public bool IsExportedToWinRT => throw new PlatformNotSupportedException();

        /// <summary>Value type is not tightly packed.</summary>
        public bool NotTightlyPacked => (VMFlags & VMFlags.NotTightlyPacked) != 0;

        /// <summary>Type contains method implementations.</summary>
        public bool ContainsMethodImpls => (VMFlags & VMFlags.ContainsMethodImpls) != 0;

        /// <summary>Marshaling type is Inhibit.</summary>
        public bool IsMarshalingTypeInhibit => (VMFlags & VMFlags.MarshalingTypeMask) == VMFlags.MarshalingTypeInhibit;

        /// <summary>Marshaling type is FreeThreaded.</summary>
        public bool IsMarshalingTypeFreeThreaded => (VMFlags & VMFlags.MarshalingTypeMask) == VMFlags.MarshalingTypeFreeThreaded;

        /// <summary>Marshaling type is Standard.</summary>
        public bool IsMarshalingTypeStandard => (VMFlags & VMFlags.MarshalingTypeMask) == VMFlags.MarshalingTypeStandard;

        private static readonly BindingFlags s_declaredFieldFlags =
            BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.Instance | BindingFlags.Static;

        /// <summary>
        ///     FieldInfos for fields declared by the associated type (all visibilities,
        ///     instance and static, excluding literal/const fields which have no FieldDesc).
        /// </summary>
        /// <remarks>
        ///     When the EEClass belongs to a canonical shared generic instantiation, the
        ///     returned context is the canonical type, not a specific closed instantiation.
        /// </remarks>
        public FieldInfo[] GetFieldInfos()
        {
            Type type = MethodTable->AsType();
            FieldInfo[] fields = type.GetFields(s_declaredFieldFlags);

            int count = 0;
            foreach (FieldInfo f in fields)
                if (!f.IsLiteral)
                    count++;

            FieldInfo[] result = new FieldInfo[count];
            int i = 0;
            foreach (FieldInfo f in fields)
                if (!f.IsLiteral)
                    result[i++] = f;

            return result;
        }

        /// <summary>
        ///     Number of fields declared by the associated type.
        /// </summary>
        /// <remarks>
        ///     Uses declared managed reflection rather than scanning the native FieldDesc
        ///     list, which has no sentinel terminator. Counts declared fields only,
        ///     inherited fields are not included.
        /// </remarks>
        public int GetFieldCount()
        {
            return GetFieldInfos().Length;
        }

        /// <summary>
        ///     Gets an array of native FieldDesc pointers for fields declared by the
        ///     associated type (see <see cref="GetFieldInfos"/> for context caveats).
        /// </summary>
        public FieldDesc*[] GetFields()
        {
            FieldInfo[] infos = GetFieldInfos();
            FieldDesc*[] fieldDescs = new FieldDesc*[infos.Length];

            for (int i = 0; i < infos.Length; i++)
                fieldDescs[i] = (FieldDesc*)infos[i].FieldHandle.Value;

            return fieldDescs;
        }

        /// <summary>
        ///     Gets the FieldDesc of the declared field with the given name, or null.
        /// </summary>
        public FieldDesc* GetField(string name)
        {
            ArgumentNullException.ThrowIfNull(name);
            FieldInfo? info = MethodTable->AsType().GetField(name, s_declaredFieldFlags);

            if (info == null || info.IsLiteral)
                return null;

            return (FieldDesc*)info.FieldHandle.Value;
        }

        /// <summary>Views this EEClass as an ArrayClass (only when <see cref="HasArrayClass"/>).</summary>
        public ArrayClass* AsArrayClass()
        {
            fixed (EEClass* ptr = &this)
            {
                return (ArrayClass*)ptr;
            }
        }

        /// <summary>Views this EEClass as a DelegateEEClass (only when <see cref="HasDelegateEEClass"/>).</summary>
        public DelegateEEClass* AsDelegateEEClass()
        {
            fixed (EEClass* ptr = &this)
            {
                return (DelegateEEClass*)ptr;
            }
        }

        /// <summary>
        ///     Gets the GuidInfo for this EEClass, or creates one from the type's GUID when
        ///     no cached info exists. Returns a managed copy. The previous version
        ///     returned a pointer to a stack local.
        /// </summary>
        public GuidInfo GetOrMakeGuid()
        {
            if (HasGuidInfo && GuidInfo != null)
                return *GuidInfo;

            return new GuidInfo(MethodTable->AsType().GUID, true);
        }

        /// <summary>
        ///     Byte size of a supported vector type, Vector&lt;T&gt;, Vector64&lt;T&gt;
        ///     or Vector128&lt;T&gt; with a primitive element type. Returns 0 otherwise.
        /// </summary>
        private static readonly Type[] s_vectorElementTypes =
        {
            typeof(sbyte), typeof(byte), typeof(short), typeof(ushort),
            typeof(int), typeof(uint), typeof(long), typeof(ulong),
            typeof(float), typeof(double), typeof(nint), typeof(nuint)
        };

        public int GetVectorSize()
        {
            Type type = MethodTable->AsType();

            if (!type.IsGenericType)
                return 0;

            Type[] args = type.GetGenericArguments();
            if (args.Length != 1 || !s_vectorElementTypes.Contains(args[0]))
                return 0;

            Type def = type.GetGenericTypeDefinition();

            if (def == typeof(System.Numerics.Vector<>))
                return MethodTable->GetNumInstanceFieldBytes();
            if (def == typeof(System.Runtime.Intrinsics.Vector64<>))
                return 8;
            if (def == typeof(System.Runtime.Intrinsics.Vector128<>))
                return 16;

            // Vector256<T> is not a .NET 8 CoreCLR native vector size here.
            return 0;
        }

        /// <summary>
        ///     HFA element type is always NONE on the supported x64 ABIs (HFA classification
        ///     is ARM64-only).
        /// </summary>
        public CorInfoHFAElemType GetHFAElemType()
        {
            return CorInfoHFAElemType.CORINFO_HFA_ELEM_NONE;
        }

        /// <summary>Gets the CharSet implied by the string-format flags.</summary>
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
