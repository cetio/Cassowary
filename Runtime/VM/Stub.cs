using Cassowary.Attributes;
using System.Runtime.InteropServices;

namespace Cassowary.Runtime.VM
{
    /// <summary>m_numCodeBytesAndFlags bit layout (stublink.h).</summary>
    public enum StubCodeFlags : uint
    {
        MULTICAST_DELEGATE_BIT = 0x80000000,
        EXTERNAL_ENTRY_BIT = 0x40000000,
        LOADER_HEAP_BIT = 0x20000000,
        INSTANTIATING_STUB_BIT = 0x10000000,
        UNWIND_INFO_BIT = 0x08000000,
        THUNK_BIT = 0x04000000,

        CODEBYTES_MASK = THUNK_BIT - 1,
        MAX_CODEBYTES = CODEBYTES_MASK + 1,
    };

    /// <summary>
    ///     Native stub view: refcount at 0, code-bytes-and-flags uint at 4, and a union
    ///     holding PatchOffset (ushort) or InstantiatedMethod (pointer) at offset 8.
    /// </summary>
    [UseAsPointer]
    [Intrinsic]
    [StructLayout(LayoutKind.Explicit)]
    public readonly unsafe struct Stub
    {
        /// <summary>Reference count (m_refcount).</summary>
        [FieldOffset(0)]
        public readonly uint RefCount;

        /// <summary>Raw code-byte-count and flags word (m_numCodeBytesAndFlags).</summary>
        [FieldOffset(4)]
        public readonly uint CodeFlagsAndBytes;

        /// <summary>Union member at 8: patch offset (USHORT).</summary>
        [FieldOffset(8)]
        public readonly ushort PatchOffset;

        /// <summary>Union member at 8: instantiated method pointer.</summary>
        [FieldOffset(8)]
        public readonly MethodDesc* InstantiatedMethod;

        /// <summary>Number of code bytes with the flag bits masked off.</summary>
        public uint NumCodeBytes
        {
            get
            {
                return CodeFlagsAndBytes & (uint)StubCodeFlags.CODEBYTES_MASK;
            }
        }

        public bool IsMulticastDelegate => (CodeFlagsAndBytes & (uint)StubCodeFlags.MULTICAST_DELEGATE_BIT) != 0;

        public bool HasExternalEntryPoint => (CodeFlagsAndBytes & (uint)StubCodeFlags.EXTERNAL_ENTRY_BIT) != 0;

        public bool IsLoaderHeap => (CodeFlagsAndBytes & (uint)StubCodeFlags.LOADER_HEAP_BIT) != 0;

        public bool IsIntantiatingStub => (CodeFlagsAndBytes & (uint)StubCodeFlags.INSTANTIATING_STUB_BIT) != 0;

        public bool HasUnwindInfo => (CodeFlagsAndBytes & (uint)StubCodeFlags.UNWIND_INFO_BIT) != 0;

        public bool HasThunk => (CodeFlagsAndBytes & (uint)StubCodeFlags.THUNK_BIT) != 0;
    }
}
