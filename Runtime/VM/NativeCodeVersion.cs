using Cassowary.Attributes;
using System.Runtime.InteropServices;

namespace Cassowary.Runtime.VM
{
    /// <summary>NativeCodeVersion StorageKind (codeversion.h).</summary>
    public enum StorageKind
    {
        Unknown,
        Explicit,
        Synthetic
    };

    /// <summary>
    ///     NativeCodeVersion view (16 bytes): a 4 byte storage kind at 0 followed by an
    ///     8 byte pointer union at 8 (version node or synthetic payload).
    /// </summary>
    [Intrinsic]
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public readonly unsafe struct NativeCodeVersion
    {
        /// <summary>The storage kind (m_storageKind, 4 byte enum).</summary>
        [FieldOffset(0)]
        public readonly StorageKind StorageKind;

        /// <summary>Union member at 8: pointer to the version node (m_pVersionNode).</summary>
        [FieldOffset(8)]
        public readonly void* VersionNode;

        /// <summary>Union member at 8: synthetic version MethodDesc pointer (m_synthetic.m_pMethodDesc).</summary>
        [FieldOffset(8)]
        public readonly MethodDesc* SyntheticMethodDesc;
    }
}
