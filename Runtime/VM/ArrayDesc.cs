using Cassowary.Attributes;
using System.Runtime.InteropServices;

namespace Cassowary.Runtime.VM
{
    // I don't like having this struct, especially since it doesn't actually exist in the Clr (it's a modified ArrayObject)
    // However, I'm too lazy to implement ArrayObject and StringObject, so it will stay.
    [Intrinsic]
    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct ArrayDesc
    {
        [FieldOffset(0)]
        public readonly int Length; // m_length

        [FieldOffset(4)]
        public readonly int Padding; // m_padding

        [FieldOffset(8)]
        internal readonly int ElemSize;

        [FieldOffset(12)]
        internal readonly int Rank;

        /// <summary>
        /// This is used in <see cref="MethodTable"/> to determine if an ArrayDesc is present.
        /// </summary>
        [FieldOffset(16)]
        internal byte Contained;

        public ArrayDesc(byte length, int size, int rank)
        {
            Length = length;
            Padding = 0;
            ElemSize = size;
            Rank = rank;
        }

        public override string ToString()
        {
            return $"ArrayDesc[{Length}]";
        }
    }
}
