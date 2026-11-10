using Cassowary.Attributes;
using JetBrains.Annotations;
using System.Runtime.InteropServices;

namespace Cassowary.Runtime.VM.EE
{
    /// <summary>
    ///     LayoutEEClass view: an EEClassLayoutInfo value is embedded immediately after the
    ///     platform dependent base (Windows 72 / Linux 56 bytes), followed by the native
    ///     layout info pointer.
    /// </summary>
    [UseAsPointer]
    [Intrinsic]
    [StructLayout(LayoutKind.Explicit)]
    public readonly unsafe struct LayoutEEClass
    {
        /// <summary>
        ///     The layout information of the EEClass (embedded m_LayoutInfo).
        /// </summary>
        public ref readonly EEClassLayoutInfo LayoutInfo
        {
            get
            {
                fixed (LayoutEEClass* ptr = &this)
                {
                    return ref Unsafe.AsRef<EEClassLayoutInfo>((byte*)ptr + EEClass.BaseSize);
                }
            }
        }

        /// <summary>
        ///     The native layout information of the EEClass (m_nativeLayoutInfo pointer).
        /// </summary>
        [CanBeNull]
        public EEClassNativeLayoutInfo* NativeLayoutInfo
        {
            get
            {
                fixed (LayoutEEClass* ptr = &this)
                {
                    return *(EEClassNativeLayoutInfo**)((byte*)ptr + EEClass.BaseSize + sizeof(void*));
                }
            }
        }

        /// <summary>
        ///     Checks whether the EEClass has native layout information.
        /// </summary>
        public bool HasNativeLayoutInfo
        {
            get
            {
                return NativeLayoutInfo != null;
            }
        }
    }
}
