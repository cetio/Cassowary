using Cassowary.Attributes;
using System.Runtime.InteropServices;

namespace Cassowary.Runtime.VM.EE
{
    /// <summary>
    ///     ComCallWrapperTemplate view (comcallablewrapper.h). COM interop only, this
    ///     structure only exists under FEATURE_COMINTEROP (Windows x64).
    /// </summary>
    [UseAsPointer]
    [Intrinsic]
    [StructLayout(LayoutKind.Explicit)]
    public readonly unsafe struct CCWTemplate
    {
        /// <summary>Reference count (m_cbRefCount, LONG at 0).</summary>
        [FieldOffset(0)]
        public readonly int RefCount;

        /// <summary>Parent template (m_pParent).</summary>
        [FieldOffset(8)]
        public readonly CCWTemplate* Parent;

        /// <summary>Class TypeHandle, raw native handle (m_thClass).</summary>
        [FieldOffset(16)]
        public readonly nint ClassTypeHandle;

        /// <summary>Default interface MethodTable (m_pDefaultItf).</summary>
        [FieldOffset(24)]
        public readonly MethodTable* DefaultInterface;

        /// <summary>Class COM method table (m_pClassComMT).</summary>
        [FieldOffset(32)]
        public readonly void* ClassComMethodTable;

        /// <summary>Basic COM method table (m_pBasicComMT).</summary>
        [FieldOffset(40)]
        public readonly void* BasicComMethodTable;

        /// <summary>Flags (m_flags, DWORD at 48).</summary>
        [FieldOffset(48)]
        public readonly uint Flags;

        /// <summary>ICustomQueryInterface::GetInterface MethodDesc (m_pICustomQueryInterfaceGetInterfaceMD).</summary>
        [FieldOffset(56)]
        public readonly MethodDesc* ICustomQueryInterfaceGetInterfaceMD;

        /// <summary>Interface count (m_cbInterfaces, ULONG at 64).</summary>
        [FieldOffset(64)]
        public readonly uint InterfaceCount;

        /// <summary>
        ///     First entry of the inline interface pointer array (m_rgpIPtr[1] at offset 72).
        /// </summary>
        public void* FirstInterfacePtr
        {
            get
            {
                fixed (CCWTemplate* ptr = &this)
                {
                    return *(void**)((byte*)ptr + 72);
                }
            }
        }
    }
}
