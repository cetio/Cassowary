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

using Cassowary.Factories;
using JetBrains.Annotations;
using System.Reflection;

namespace Cassowary.Intrinsics
{
    /// <summary>
    /// Provides easy access to native method signatures and invocation.
    /// </summary>
    public sealed class Signature
    {
        private static Func<object?, nint, dynamic, bool, object> _invokerInternal =
            Unsafe.As<Func<object?, nint, dynamic, bool, object>>(DelegateFactory.MakeDelegate(typeof(RuntimeMethodHandle)
            .GetMethod("InvokeMethod", BindingFlags.NonPublic | BindingFlags.Static)!));

        private static Func<dynamic, dynamic, object> _creatorInternal = 
            Unsafe.As<Func<dynamic, dynamic, object>>(DelegateFactory.MakeConstructorDelegate(
                Type.GetType("System.Signature")!, 
                Type.GetType("System.Signature")!.GetConstructors()[1]!, 
                typeof(object), typeof(object)));

        private object _nativeSignature;
        private bool _isConstructor;

        private Signature(object nativeSignature, bool isConstructor, bool typeSafetyCheck = true)
        {
            if (typeSafetyCheck && nativeSignature.GetType() != Type.GetType("System.Signature"))
            {
                throw new ArgumentException("'nativeSignature' argument is not of type System.Signature");
            }

            _nativeSignature = nativeSignature;
            _isConstructor = isConstructor;
        }

        /// <summary>
        /// Gets a signature safely from the given MethodInfo.
        /// </summary>
        /// <param name="methodInfo">The MethodInfo to get the signature from.</param>
        /// <returns>The signature of the provided MethodInfo.</returns>
        public static Signature GetSignatureSafe(MethodInfo methodInfo)
        {
            object rtMethodInfo = Intrinsics.AsRuntimeMethodInfo(methodInfo);
            PropertyInfo signatureProperty = rtMethodInfo.GetType().GetProperty(
                "Signature",
                BindingFlags.NonPublic | BindingFlags.Instance)!;

            if (signatureProperty == null)
                throw new InvalidOperationException("Failed to retrieve the 'Signature' property.");

            object rtSignature = signatureProperty.GetValue(rtMethodInfo)!;
            return new Signature(rtSignature, false, false);
        }

        /// <summary>
        /// Gets a signature safely from the given ConstructorInfo.
        /// </summary>
        /// <param name="ctorInfo">The ConstructorInfo to get the signature from.</param>
        /// <returns>The signature of the provided ConstructorInfo.</returns>
        public static Signature GetSignatureSafe(ConstructorInfo ctorInfo)
        {
            object rtMethodInfo = Intrinsics.AsRuntimeConstructorInfo(ctorInfo);
            PropertyInfo signatureProperty = rtMethodInfo.GetType().GetProperty(
                "Signature",
                BindingFlags.NonPublic | BindingFlags.Instance)!;

            if (signatureProperty == null)
                throw new InvalidOperationException("Failed to retrieve the 'Signature' property.");

            object rtSignature = signatureProperty.GetValue(rtMethodInfo)!;
            return new Signature(rtSignature, true, false);
        }

        /// <summary>
        /// Gets a signature unsafely from the given MethodInfo.
        /// </summary>
        /// <param name="methodInfo">The MethodInfo to get the signature from.</param>
        /// <returns>The signature of the provided MethodInfo.</returns>
        public static Signature GetSignatureUnsafe(MethodInfo methodInfo)
        {
            dynamic rtMethodInfo = Intrinsics.AsRuntimeMethodInfo(methodInfo);
            return new Signature(_creatorInternal(rtMethodInfo, rtMethodInfo.DeclaringType), false, false);
        }

        /// <summary>
        /// Gets a signature unsafely from the given MethodInfo.
        /// </summary>
        /// <param name="methodInfo">The MethodInfo to get the signature from.</param>
        /// <returns>The signature of the provided MethodInfo.</returns>
        public static Signature GetSignatureUnsafe(ConstructorInfo ctorInfo)
        {
            dynamic rtCtorInfo = Intrinsics.AsRuntimeConstructorInfo(ctorInfo);
            return new Signature(_creatorInternal(rtCtorInfo, rtCtorInfo.DeclaringType), true, false);
        }

        /// <summary>
        /// Gets or sets the value of the native signature object.
        /// </summary>
        [NotNull]
        public object Value
        {
            get
            {
                return _nativeSignature;
            }
            set
            {
                if (value.GetType() != Type.GetType("System.Signature"))
                    throw new ArgumentException("Value is not of type System.Signature");

                _nativeSignature = value;
            }
        }

        /// <summary>
        /// Invokes the method associated with this Signature.
        /// </summary>
        /// <param name="instance">The instance to invoke on.</param>
        /// <param name="parameters">The parameters to invoke the method with.</param>
        /// <returns>The return of the invocation.</returns>
        public unsafe object Invoke(object instance, params object[] parameters)
        {
            if (_isConstructor)
            {
                object ctord = _invokerInternal(instance, (nint)Intrinsics.GetPointerArray(parameters), _nativeSignature, _isConstructor);
                uint size = (uint)Intrinsics.GetMethodTable(instance)->GetNumInstanceFieldBytes();
                Unsafe.CopyBlock(ref Intrinsics.GetData(instance), ref Intrinsics.GetData(ctord), size);
                return ctord;
            }
                
            return _invokerInternal(instance, (nint)Intrinsics.GetPointerArray(parameters), _nativeSignature, _isConstructor);
        }

        /// <summary>
        /// Invokes the method associated with this Signature.
        /// </summary>
        /// <param name="parameters">The parameters to invoke the method with.</param>
        /// <returns>The return of the invocation.</returns>
        public unsafe object Invoke(params object[] parameters)
        {
            return _invokerInternal(null, (nint)Intrinsics.GetPointerArray(parameters), _nativeSignature, _isConstructor);
        }
    }
}
