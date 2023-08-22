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

using JetBrains.Annotations;
using System.Reflection;

namespace Cassowary.Intrinsics
{
    public sealed class Signature
    {
        private object _nativeSignature;

        /// <summary>
        /// Initializes a new instance of the <see cref="Signature"/> struct.
        /// </summary>
        /// <param name="nativeSignature">The native signature object.</param>
        /// <param name="typeSafetyCheck">Flag indicating whether to perform type safety check.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="typeSafetyCheck"/> is enabled and 
        /// <paramref name="nativeSignature"/> is not of type <see cref="System.Signature"/>.</exception>
        public Signature(object nativeSignature, bool typeSafetyCheck = true)
        {
            if (typeSafetyCheck && nativeSignature.GetType() != Type.GetType("System.Signature"))
            {
                throw new ArgumentException("'nativeSignature' argument is not of type System.Signature");
            }

            _nativeSignature = nativeSignature;
        }

        /// <summary>
        /// Gets a safe signature from the given method information.
        /// </summary>
        /// <param name="methodInfo">The method information.</param>
        /// <returns>The safe <see cref="Signature"/> instance.</returns>
        public static Signature GetSignatureSafe(MethodInfo methodInfo)
        {
            object rtMethodInfo = Intrinsics.AsRuntimeMethodInfo(methodInfo);
            var signatureProperty = rtMethodInfo.GetType().GetProperty(
                "Signature",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (signatureProperty == null)
                throw new InvalidOperationException("Failed to retrieve the 'Signature' property.");

            var rtSignature = signatureProperty.GetValue(rtMethodInfo);
            return new Signature(rtSignature);
        }

        /// <summary>
        /// Gets or sets the value of the native signature object.
        /// </summary>
        [NotNull]
        public object Value
        {
            get => _nativeSignature;
            set
            {
                if (value.GetType() != Type.GetType("System.Signature"))
                    throw new ArgumentException("Value is not of type System.Signature");

                _nativeSignature = value;
            }
        }
    }
}
