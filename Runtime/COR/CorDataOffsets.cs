namespace Cassowary.Runtime.Cor
{
    public unsafe class CorDataOffsets
    {
        /// <remarks>
        /// LENGTH -> PADDING -> DATA
        /// </remarks>
        public const int OFFSET_ARRAY = sizeof(uint) + sizeof(uint);
        /// <remarks>
        /// LENGTH -> DATA
        /// </remarks>
        public const int OFFSET_STRING = sizeof(uint);
        /// <remarks>
        /// LENGTH -> PADDING -> DATA
        /// </remarks>
        public const int OFFSET_PTRARRAY = sizeof(uint) + sizeof(uint);
        /// <remarks>
        /// KEEPALIVE -> CACHE -> TYPEHANDLE -> DATA
        /// </remarks>
        public readonly int OFFSET_REFLECTION = sizeof(object) + sizeof(object) + sizeof(nint);
        /// <remarks>
        /// DATAMAP -> DATA
        /// </remarks>
        public readonly int OFFSET_COM = sizeof(object);
    }
}
