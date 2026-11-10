using Cassowary.Attributes;
using System.Runtime.InteropServices;

namespace Cassowary.Runtime.VM.EE
{
    [UseAsPointer]
    [Intrinsic]
    [StructLayout(LayoutKind.Explicit)]
    public readonly unsafe struct CCWTemplate
    {
        /// <summary>
        /// Pointer to the VTable of the COM interface.
        /// </summary>
        [FieldOffset(0)]
        public readonly byte* VTable; //m_pVtable

        /// <summary>
        /// Pointer to the MethodTable of the wrapped object.
        /// </summary>
        [FieldOffset(8)]
        public readonly MethodTable* WrappedMethodTable; //m_pWrappedMethodTable

        /// <summary>
        /// Pointer to the MethodDesc of the wrapped object's method.
        /// </summary>
        [FieldOffset(16)]
        public readonly MethodDesc* WrappedMethodDesc; //m_pMDesc

        /// <summary>
        /// Pointer to the MLHeader structure.
        /// </summary>
        [FieldOffset(24)]
        public readonly byte* MLHeader; //m_pMLHeader

        /// <summary>
        /// Pointer to the MLCode structure.
        /// </summary>
        [FieldOffset(32)]
        public readonly byte* MLCode; //m_pMLCode

        /// <summary>
        /// Pointer to the MethodDescChunk of the wrapped object's method.
        /// </summary>
        [FieldOffset(40)]
        public readonly MethodDescChunk* MethodDescChunk; //m_pMDescChunk

        /// <summary>
        /// Pointer to the ILCode structure.
        /// </summary>
        [FieldOffset(48)]
        public readonly byte* ILCode; //m_pILCode

        /// <summary>
        /// Pointer to extra information (if needed). This functions similarly to MultiPurposeSlot.
        /// </summary>
        [FieldOffset(56)]
        public readonly byte* ExtraInfo; //m_pExtraInfo 

        /// <summary>
        /// Pointer to the MethodTable of the COM interface.
        /// </summary>
        [FieldOffset(64)]
        public readonly MethodTable* InterfaceMethodTable; // m_pInterfaceMT
    }
}
