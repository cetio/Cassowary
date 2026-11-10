using Cassowary.Attributes;
using System.Runtime.InteropServices;

namespace Cassowary.Runtime.VM
{
    /// <summary>
    ///     UMThunkMarshInfo view (dllimportcallback.h): IL stub at 0, MethodDesc at 8,
    ///     Module at 16, and the native Signature blob pointer plus length at 24 and 32.
    /// </summary>
    [UseAsPointer]
    [Intrinsic]
    [StructLayout(LayoutKind.Explicit)]
    public readonly unsafe struct UMThunkMarshInfo
    {
        /// <summary>IL stub code pointer (m_pILStub).</summary>
        [FieldOffset(0)]
        public readonly byte* ILStub;

        /// <summary>MethodDesc pointer, may be null (m_pMD).</summary>
        [FieldOffset(8)]
        public readonly MethodDesc* MethodDesc;

        /// <summary>Module pointer (m_pModule).</summary>
        [FieldOffset(16)]
        public readonly byte* Module;

        /// <summary>Native signature blob pointer (m_sig, PCCOR_SIGNATURE at 24).</summary>
        [FieldOffset(24)]
        public readonly byte* Signature;

        /// <summary>Signature blob length in bytes (at 32).</summary>
        [FieldOffset(32)]
        public readonly uint SignatureLength;

        /// <summary>
        ///     Member offset of m_pILStub within the structure. Always 0, matching the
        ///     native GetOffsetOfStub which returns offsetof rather than an address difference.
        /// </summary>
        public int OffsetOfStub => 0;

        /// <summary>
        ///     Gets a value determining whether or not the associated MethodDesc or Stub is fully initialized.
        /// </summary>
        public bool IsCompletelyInited()
        {
            return ILStub != (byte*)1;
        }
    }
}
