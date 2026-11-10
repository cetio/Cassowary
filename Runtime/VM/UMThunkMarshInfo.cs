using Cassowary.Attributes;
using System.Runtime.InteropServices;

namespace Cassowary.Runtime.VM
{
    [UseAsPointer]
    [Intrinsic]
    [StructLayout(LayoutKind.Explicit)]
    public readonly unsafe struct UMThunkMarshInfo
    {
        [FieldOffset(0)]
        public readonly byte* ILStub; // m_pILStub

        [FieldOffset(8)]
        public readonly MethodDesc* MethodDesc; // m_pMD

        [FieldOffset(16)]
        public readonly byte* Module; // m_pModule

        [FieldOffset(24)]
        public readonly uint Signature; // m_sig

        /// <summary>
        /// Gets the offset from this structure to the start of the associated IL Stub.
        /// </summary>
        public int OffsetOfStub
        {
            get
            {
                fixed (UMThunkMarshInfo* ptr = &this)
                {
                    return (int)((byte*)ptr - ILStub);
                }
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated MethodDesc or Stub is fully initialized.
        /// </summary>
        public bool IsCompletelyInited()
        {
            return ILStub != (byte*)1;
        }
    }
}
