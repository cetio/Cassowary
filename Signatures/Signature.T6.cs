using System.Reflection;

namespace Cassowary.Signatures
{
    /// <summary>
    /// Provides easy access to native method signatures and invocation.
    /// </summary>
    public sealed class Signature<T1, T2, T3, T4, T5, TReturn> : BaseSignature
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
        public unsafe TReturn Invoke<T>(ref T instance, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
        {
            return (TReturn)InvokeInternal(ref instance, this, param1, param2, param3, param4, param5);
        }

        /// <summary>
        /// Invokes the method associated with this Signature.
        /// </summary>
        /// <remarks>
        /// To invoke on an instance, use <see cref="Invoke{T}(ref T, T1, T2, T3, T4, T5)"/>
        /// </remarks>
        /// <returns>The return of the invocation.</returns>
        public unsafe TReturn Invoke(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
        {
            return (TReturn)InvokeInternal(this, param1, param2, param3, param4, param5);
        }
    }
}
