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

namespace Cassowary.Intrinsics.VM.Cor
{
    [Intrinsic]
    public enum CorInterfaceType
    {
        /// <summary>
        /// Indicates that the interface is exposed to COM as a dual interface, which enables both early and late binding.
        /// </summary>
        Dual = 0,

        /// <summary>
        /// Indicates that an interface is exposed to COM as an interface that is derived from IUnknown, which enables only early binding.
        /// </summary>
        IUnknown = 1,

        /// <summary>
        /// Indicates that an interface is exposed to COM as a dispinterface, which enables late binding only.
        /// </summary>
        IDispatch = 2,

        /// <summary>
        /// Indicates that an interface is exposed to COM as a Windows Runtime interface.
        /// </summary>
        IInspectable = 3,

        /// <summary>
        /// Represents the IInspectable ABI interface.
        /// </summary>
        IInspectableABI = 4,

        /// <summary>
        /// Represents the IInspectable Vftbl (Virtual Function Table) interface.
        /// </summary>
        IInspectableVftbl = 5,

        /// <summary>
        /// Represents the IMarshal interface.
        /// </summary>
        IMarshal = 6
    }
}
