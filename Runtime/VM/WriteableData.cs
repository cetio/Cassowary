using Cassowary.Attributes;
using System.Runtime.InteropServices;

namespace Cassowary.Runtime.VM
{
    [Flags]
    public enum WriteableFlags : uint
    {
        Unrestored = 0x00000004,
        HasApproxParent = 0x00000010,
        UnrestoredTypeKey = 0x00000020,
        IsNotFullyLoaded = 0x00000040,
        DependenciesLoaded = 0x00000080,     // class and all dependencies loaded up to CLASS_LOADED_BUT_NOT_VERIFIED

        CanCompareBitsOrUseFastGetHashCode = 0x00000200,     // Is any field type or sub field type overrode Equals or GetHashCode
        HasCheckedCanCompareBitsOrUseFastGetHashCode = 0x00000400,  // Whether we have checked the overridden Equals or GetHashCode

        ParentMethodTablePointerValid = 0x40000000,
        HasInjectedInterfaceDuplicates = 0x80000000,
    }

    [Intrinsic]
    [StructLayout(LayoutKind.Explicit)]
    public readonly unsafe struct WriteableData
    {
        /// <summary>
        /// Flags pertaining to the parent structure (MethodTable.)
        /// </summary>
        [FieldOffset(0)]
        public readonly WriteableFlags WriteableFlags; // m_dwFlags

        //[FieldOffset(4)]
        //public readonly RuntimeTypeHandle m_hExposedClassObject;

        /// <summary>
        /// Gets a value determining whether or not the associated MethodTable is unrestored.
        /// </summary>
        public bool IsUnrestored
        {
            get
            {
                return WriteableFlags.HasFlag(WriteableFlags.Unrestored);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated MethodTable is has an approximate parent.
        /// </summary>
        public bool HasApproxParent
        {
            get
            {
                return WriteableFlags.HasFlag(WriteableFlags.HasApproxParent);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated MethodTable has any unrestored type key.
        /// </summary>
        public bool HasUnrestoredTypeKey
        {
            get
            {
                return WriteableFlags.HasFlag(WriteableFlags.UnrestoredTypeKey);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated MethodTable is not fully loaded.
        /// </summary>
        public bool IsNotFullyLoaded
        {
            get
            {
                return WriteableFlags.HasFlag(WriteableFlags.IsNotFullyLoaded);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated MethodTable has loaded its dependencies yet.
        /// </summary>
        public bool DependenciesLoaded
        {
            get
            {
                return WriteableFlags.HasFlag(WriteableFlags.DependenciesLoaded);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated MethodTable can compare bits or use FastGetHashCode().
        /// </summary>
        public bool CanCompareBitsOrUseFastGetHashCode
        {
            get
            {
                return WriteableFlags.HasFlag(WriteableFlags.CanCompareBitsOrUseFastGetHashCode);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the associated MethodTable checked if it can compare bits or use FastGetHashCode().
        /// </summary>
        public bool HasCheckedCanCompareBitsOrUseFastGetHashCode
        {
            get
            {
                return WriteableFlags.HasFlag(WriteableFlags.HasCheckedCanCompareBitsOrUseFastGetHashCode);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not the parent method table pointer is valid.
        /// </summary>
        public bool ParentMethodTablePointerValid
        {
            get
            {
                return WriteableFlags.HasFlag(WriteableFlags.ParentMethodTablePointerValid);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not there are injected interface duplicates.
        /// </summary>
        public bool HasInjectedInterfaceDuplicates
        {
            get
            {
                return WriteableFlags.HasFlag(WriteableFlags.HasInjectedInterfaceDuplicates);
            }
        }
    }
}
