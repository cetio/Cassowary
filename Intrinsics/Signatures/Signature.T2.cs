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

using System.Reflection;

namespace Cassowary.Intrinsics
{
    /// <summary>
    /// Provides easy access to native method signatures and invocation.
    /// </summary>
    public sealed class Signature<T1, TReturn> : BaseSignature
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
        /// <returns>The return of the invocation.</returns>
        public unsafe TReturn Invoke<T>(ref T instance, T1 param1)
        {
            return (TReturn)InvokeInternal(ref instance, this, param1);
        }

        /// <summary>
        /// Invokes the method associated with this Signature.
        /// </summary>
        /// <remarks>
        /// To invoke on an instance, use <see cref="Invoke{T}(ref T, T1)"/>
        /// </remarks>
        /// <returns>The return of the invocation.</returns>
        public unsafe TReturn Invoke(T1 param1)
        {
            return (TReturn)InvokeInternal(this, param1);
        }
    }
}
