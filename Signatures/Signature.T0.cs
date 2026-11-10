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
