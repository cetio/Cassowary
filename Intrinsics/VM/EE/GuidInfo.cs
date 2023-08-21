//              FUCK OOP & FUCK INTERLACED LIBRARIES
//
//            DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//                   Version 2, December 2004
// 
// Copyright (C) 2004 Sam Hocevar<sam@hocevar.net>
//
// Everyone is permitted to copy and distribute verbatim or modified
// copies of this license document, and changing it is allowed as long
// as the name is changed.
//
//           DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//  TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION
//
//  0. You just DO WHAT THE FUCK YOU WANT TO.

using Cassowary.Attributes;
using System.Runtime.InteropServices;

namespace Cassowary.Intrinsics.VM.EE
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
