using Cassowary.Attributes;
using System.Runtime.InteropServices;

namespace Cassowary.Runtime.VM.EE
{
    /// <summary>
    ///     EEClassOptionalFields view. The first two pointer fields are unconditional.
    ///     The COM-only sparse-vtable-map and coclass fields exist only under
    ///     FEATURE_COMINTEROP (Windows). Module-dynamic-ID, System V eightbyte arrays and
    ///     required-field-alignment offsets differ between the Windows-activation and
    ///     UNIX_AMD64_ABI conditional layouts, so they are not exposed.
    /// </summary>
    [UseAsPointer]
    [Intrinsic]
    [StructLayout(LayoutKind.Explicit)]
    public readonly unsafe struct EEClassOptionalFields
    {
        /// <summary>Layout of the handle dictionary for shared generic types (m_pDictLayout).</summary>
        [FieldOffset(0)]
        public readonly byte* DictionaryLayout;

        /// <summary>Variance info for each type parameter (m_pVarianceInfo).</summary>
        [FieldOffset(8)]
        public readonly byte* VarianceInfo;

        /// <summary>SparseVTableMap pointer. COM interop only (Windows), offset 16.</summary>
        /// <exception cref="PlatformNotSupportedException">Non-Windows runtime.</exception>
        public SparseVTableMap* SparseVTableMap
        {
            get
            {
                if (!RuntimeSupport.IsWindows)
                    throw new PlatformNotSupportedException("m_pSparseVTableMap exists only under FEATURE_COMINTEROP (Windows).");

                fixed (EEClassOptionalFields* ptr = &this)
                {
                    return *(SparseVTableMap**)((byte*)ptr + 16);
                }
            }
        }

        /// <summary>
        ///     Coclass TypeHandle for an interface (raw native TypeHandle, not a managed
        ///     RuntimeTypeHandle). COM interop only (Windows), offset 24.
        /// </summary>
        /// <exception cref="PlatformNotSupportedException">Non-Windows runtime.</exception>
        public nint CoClass
        {
            get
            {
                if (!RuntimeSupport.IsWindows)
                    throw new PlatformNotSupportedException("m_pCoClassForIntf exists only under FEATURE_COMINTEROP (Windows).");

                fixed (EEClassOptionalFields* ptr = &this)
                {
                    return Unsafe.Read<nint>((byte*)ptr + 24);
                }
            }
        }

        /// <summary>
        ///     Not exposed: its offset depends on the FEATURE_COMINTEROP_UNMANAGED_ACTIVATION
        ///     and UNIX_AMD64_ABI conditional layout, which is not modeled.
        /// </summary>
        /// <exception cref="PlatformNotSupportedException">Always.</exception>
        [Obsolete("m_cbModuleDynamicID offset depends on conditional native fields and is not supported.")]
        public uint ModuleDynamicID => throw new PlatformNotSupportedException();

        /// <summary>
        ///     Not exposed: its offset follows the conditional System V eightbyte arrays
        ///     on UNIX_AMD64_ABI and COM fields on Windows, not modeled.
        /// </summary>
        /// <exception cref="PlatformNotSupportedException">Always.</exception>
        [Obsolete("m_requiredFieldAlignment offset depends on conditional native fields and is not supported.")]
        public byte RequiredFieldAlignment => throw new PlatformNotSupportedException();
    }
}
