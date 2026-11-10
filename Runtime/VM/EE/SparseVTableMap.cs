using Cassowary.Attributes;
using System.Runtime.InteropServices;

namespace Cassowary.Runtime.VM.EE
{
    [UseAsPointer]
    [Intrinsic]
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct SparseVTableMap
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Entry
        {
            /// <summary>
            /// Starting MT slot number
            /// </summary>
            public ushort Start;

            /// <summary>
            /// # of consecutive slots that map linearly
            /// </summary>
            public ushort Span;

            /// <summary>
            /// Starting VT slot number
            /// </summary>
            public ushort MapTo;
        }

        /// <summary>
        /// Array of Entry structures.
        /// </summary>
        public readonly Entry* EntryMapList; // m_MapList

        /// <summary>
        /// Number of entries in the array.
        /// </summary>
        public readonly ushort NumMapEntries; // m_MapEntries

        /// <summary>
        /// Number of entries allocated.
        /// </summary>
        public readonly ushort NumAllocated; // m_Allocated

        /// <summary>
        /// Index of last entry used in successful lookup.
        /// </summary>
        public readonly ushort LastUsedIndex; // m_LastUsed

        /// <summary>
        /// Current VT slot number, used during list build
        /// </summary>
        public readonly ushort VTSlot; // m_VTSlot

        /// <summary>
        /// Current MT slot number, used during list build
        /// </summary>
        public readonly ushort MTSlot; // m_MTSlot
    }
}
