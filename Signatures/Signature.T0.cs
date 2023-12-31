﻿//              FUCK OOP & FUCK INTERLACED LIBRARIES
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

using System.Reflection;

namespace Cassowary.Signatures
{
    /// <summary>
    /// Provides easy access to native method signatures and invocation.
    /// </summary>
    public sealed class Signature : BaseSignature
    {
        /// <summary>
        /// Creates a signature from the given MethodBase.
        /// </summary>
        public Signature(MethodBase methodBase) : base(methodBase) { }

        /// <summary>
        /// Creates a signature from the given Delegate.
        /// </summary>
        public Signature(Delegate @delegate) : base(@delegate) { }

        /// <summary>
        /// Invokes the method associated with this Signature.
        /// </summary>
        /// <param name="instance">The instance to invoke on.</param>
        /// <param name="parameters">The parameters to invoke the method with.</param>
        /// <returns>The return of the invocation.</returns>
        public unsafe object Invoke<T>(ref T instance, params object[] parameters)
        {
            return InvokeInternal(ref instance, this, parameters);
        }

        /// <summary>
        /// Invokes the method associated with this Signature.
        /// </summary>
        /// <remarks>
        /// To invoke on an instance, use <see cref="Invoke{T}(ref T, object[])"/>
        /// </remarks>
        /// <param name="parameters">The parameters to invoke the method with.</param>
        /// <returns>The return of the invocation.</returns>
        public unsafe object Invoke(params object[] parameters)
        {
            return InvokeInternal(this, parameters);
        }
    }
}
