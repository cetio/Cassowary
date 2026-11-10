using Cassowary.Attributes;
using System.Runtime.InteropServices;

namespace Cassowary.Runtime.VM
{
    [Intrinsic]
    [StructLayout(LayoutKind.Sequential)]
    public sealed unsafe class RelaxedGrossObject
    {
        /// <summary>
        /// Start of the object data bytes.
        /// </summary>
        public byte Data; // m_data
    }
}