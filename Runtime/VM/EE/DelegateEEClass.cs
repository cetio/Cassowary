using Cassowary.Attributes;
using JetBrains.Annotations;

using System.Runtime.InteropServices;

namespace Cassowary.Runtime.VM.EE
{
    /// <summary>
    ///     DelegateEEClass view: pointer slots begin immediately after the platform
    ///     dependent EEClass base (Windows 72 / Linux 56 bytes).
    /// </summary>
    [UseAsPointer]
    [Intrinsic]
    [StructLayout(LayoutKind.Explicit)]
    public readonly unsafe struct DelegateEEClass
    {
        private void* Slot(int index)
        {
            fixed (DelegateEEClass* ptr = &this)
            {
                return *(void**)((byte*)ptr + EEClass.BaseSize + index * sizeof(void*));
            }
        }

        public Stub* StaticCallStub => (Stub*)Slot(0); // m_pStaticCallStub

        public Stub* InstRetBuffCallStub => (Stub*)Slot(1); // m_pInstRetBuffCallStub

        public MethodDesc* InvokeMethod => (MethodDesc*)Slot(2); // m_pInvokeMethod

        public Stub* MultiCastInvokeStub => (Stub*)Slot(3); // m_pMultiCastInvokeStub

        public Stub* WrapperDelegateInvokeStub => (Stub*)Slot(4); // m_pWrapperDelegateInvokeStub

        public UMThunkMarshInfo* UMThunkMarshInfo => (UMThunkMarshInfo*)Slot(5); // m_pUMThunkMarshInfo

        public MethodDesc* BeginInvokeMethod => (MethodDesc*)Slot(6); // m_pBeginInvokeMethod

        public MethodDesc* EndInvokeMethod => (MethodDesc*)Slot(7); // m_pEndInvokeMethod

        [CanBeNull]
        public byte* MarshalStub => (byte*)Slot(8); // m_pMarshalStub

        /// <summary>
        ///     ComPlusCallInfo pointer. COM interop only, exists only on Windows.
        /// </summary>
        /// <exception cref="PlatformNotSupportedException">Non-Windows runtime.</exception>
        [CanBeNull]
        public void* ComPlusCallInfo
        {
            get
            {
                if (!RuntimeSupport.IsWindows)
                    throw new PlatformNotSupportedException("m_pComPlusCallInfo exists only under FEATURE_COMINTEROP (Windows).");

                return Slot(9);
            }
        }
    }
}
