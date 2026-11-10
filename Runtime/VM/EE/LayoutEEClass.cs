using Cassowary.Attributes;
using JetBrains.Annotations;
using System.Runtime.InteropServices;

namespace Cassowary.Runtime.VM.EE
{
    [UseAsPointer]
    [Intrinsic]
    [StructLayout(LayoutKind.Explicit)]
    public readonly unsafe struct LayoutEEClass
    {
        /// <summary>
        /// The layout information of the EEClass.
        /// </summary>
        [FieldOffset(72)]
        public readonly EEClassLayoutInfo LayoutInfo; //m_LayoutInfo

        /// <summary>
        /// The native layout information of the EEClass.
        /// </summary>
        [CanBeNull]
        [FieldOffset(80)]
        public readonly EEClassNativeLayoutInfo* NativeLayoutInfo; //m_nativeLayoutInfo

        /// <summary>
        /// Checks whether the EEClass has native layout information.
        /// </summary>
        public bool HasNativeLayoutInfo
        {
            get
            {
                try
                {
                    if ((nint)NativeLayoutInfo == 0)
                        return false;

                    // This is just intended to cause a object reference error (if the pointer doesn't point to EEClassOptionalFields)
                    return (object?)*NativeLayoutInfo != null;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
