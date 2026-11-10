using Cassowary.Attributes;
using System.Runtime.InteropServices;

namespace Cassowary.Runtime.VM.EE
{
    /// <summary>
    ///     m_bFlags values of the native EEClassLayoutInfo (.NET 8 class.h).
    /// </summary>
    [Flags]
    public enum LayoutFlags : byte
    {
        /// <summary>GC layout is bit-for-bit identical to the unmanaged counterpart.</summary>
        Blittable = 0x01,
        /// <summary>Type is also sequential in managed memory.</summary>
        ManagedSequential = 0x02,
        /// <summary>Type was conceptually zero-sized (bumped to 1 byte natively).</summary>
        ZeroSized = 0x04,
        /// <summary>Size is explicitly specified in metadata.</summary>
        HasExplicitSize = 0x08,
        /// <summary>Type recursively has a field that is LayoutKind.Auto and not an enum.</summary>
        HasAutoLayoutFieldInLayout = 0x10,
        /// <summary>Type is Int128 or recursively has an Int128 field.</summary>
        IsOrHasInt128Field = 0x20
    }

    /// <summary>
    ///     Native EEClassLayoutInfo value (8 bytes, embedded inside a LayoutEEClass).
    /// </summary>
    [Intrinsic]
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public readonly unsafe struct EEClassLayoutInfo
    {
        /// <summary>Managed size in bytes (m_cbManagedSize).</summary>
        [FieldOffset(0)]
        public readonly int ManagedSize;

        /// <summary>
        ///     Largest managed alignment requirement of all members
        ///     (m_ManagedLargestAlignmentRequirementOfAllMembers).
        /// </summary>
        [FieldOffset(4)]
        public readonly byte ManagedLargestAlignmentRequirementOfAllMembers;

        /// <summary>Managed layout flags (m_bFlags).</summary>
        [FieldOffset(5)]
        public readonly LayoutFlags LayoutFlags;

        /// <summary>Packing size in bytes (m_cbPackingSize).</summary>
        [FieldOffset(6)]
        public readonly byte PackingSize;

        public bool IsBlittable => (LayoutFlags & LayoutFlags.Blittable) != 0;

        public bool IsManagedSequential => (LayoutFlags & LayoutFlags.ManagedSequential) != 0;

        public bool IsZeroSized => (LayoutFlags & LayoutFlags.ZeroSized) != 0;

        public bool HasExplicitSize => (LayoutFlags & LayoutFlags.HasExplicitSize) != 0;

        public bool HasAutoLayoutField => (LayoutFlags & LayoutFlags.HasAutoLayoutFieldInLayout) != 0;

        public bool IsInt128OrHasInt128Fields => (LayoutFlags & LayoutFlags.IsOrHasInt128Field) != 0;
    }
}
