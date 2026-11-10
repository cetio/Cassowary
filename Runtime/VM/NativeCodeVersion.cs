using Cassowary.Attributes;
using System.Runtime.InteropServices;

namespace Cassowary.Runtime.VM
{
    public enum StorageKind
    {
        Unknown,
        Explicit,
        Synthetic
    };

    [Intrinsic]
    [StructLayout(LayoutKind.Explicit)]
    public readonly unsafe struct NativeCodeVersion
    {
        /// <summary>
        /// The storage kind of the associated MethodDesc.
        /// </summary>
        [FieldOffset(0)]
        public readonly StorageKind StorageKind; // m_storageKind
    }
}
