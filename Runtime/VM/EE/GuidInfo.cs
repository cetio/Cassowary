using Cassowary.Attributes;
using System.Runtime.InteropServices;

namespace Cassowary.Runtime.VM.EE
{
    [Intrinsic]
    [StructLayout(LayoutKind.Explicit)]
    public readonly unsafe struct GuidInfo
    {
        /// <summary>
        /// The actual guid of the type.
        /// </summary>
        [FieldOffset(0)]
        public readonly Guid Guid; //m_Guid

        /// <summary>
        /// A boolean indicating if it was generated from the name of the type.
        /// </summary>
        [FieldOffset(16)]
        public readonly bool GeneratedFromName; //m_bGeneratedFromName

        internal GuidInfo(Guid guid, bool generatedFromName)
        {
            Guid = guid;
            GeneratedFromName = generatedFromName;
        }
    }

}
