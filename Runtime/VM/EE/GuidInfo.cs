using Cassowary.Attributes;
using System.Runtime.InteropServices;

namespace Cassowary.Runtime.VM.EE
{
    /// <summary>
    ///     Native GuidInfo (GUID at 0, native BOOL at 16, a 4-byte value).
    /// </summary>
    [Intrinsic]
    [StructLayout(LayoutKind.Explicit)]
    public readonly unsafe struct GuidInfo
    {
        /// <summary>The actual guid of the type (m_Guid).</summary>
        [FieldOffset(0)]
        public readonly Guid Guid;

        /// <summary>Native BOOL indicating the guid was generated from the type name.</summary>
        [FieldOffset(16)]
        internal readonly int GeneratedFromNameNative;

        /// <summary>Whether the guid was generated from the name of the type.</summary>
        public bool GeneratedFromName => GeneratedFromNameNative != 0;

        internal GuidInfo(Guid guid, bool generatedFromName)
        {
            Guid = guid;
            GeneratedFromNameNative = generatedFromName ? 1 : 0;
        }
    }
}
