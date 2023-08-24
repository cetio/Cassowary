using System.Reflection;
using System.Runtime.InteropServices;

namespace Cassowary.Intrinsics
{
    public abstract class BaseSignature
    {
        private static Func<object?, nint, dynamic, bool, object> _invokeInternal =
            Unsafe.As<Func<object?, nint, dynamic, bool, object>>(DelegateFactory.MakeDelegate(typeof(RuntimeMethodHandle)
            .GetMethod("InvokeMethod", BindingFlags.NonPublic | BindingFlags.Static)!));

        internal static Func<MethodBase, Type, object> CreateInternal =
            Unsafe.As<Func<MethodBase, Type, object>>(DelegateFactory.MakeConstructorDelegate(
            Type.GetType("System.Signature")!,
            Type.GetType("System.Signature")!.GetConstructors()[1]!,
            typeof(object), typeof(object)));

        public object NativeSignature { get; internal set; }
        public bool IsCtor { get; internal set; }

        /// <summary>
        /// Creates a signature from the given MethodBase.
        /// </summary>
        public BaseSignature(MethodBase methodBase)
        {
            // I won't add a check for if the declaring type is null, since 99.999% of the time, it won't be,
            // and if it is, it shouldn't cause any problems as far as I know.
            NativeSignature = CreateInternal(methodBase, methodBase.DeclaringType!);
            IsCtor = methodBase.IsConstructor;
        }

        /// <summary>
        /// Creates a signature from the given Delegate.
        /// </summary>
        public BaseSignature(Delegate @delegate)
        {
            // I won't add a check for if the declaring type is null, since 99.999% of the time, it won't be,
            // and if it is, it shouldn't cause any problems as far as I know.
            NativeSignature = CreateInternal(@delegate.Method, @delegate.Method.DeclaringType!);
            IsCtor = false; // A delegate cannot be a constructor.
        }

        /// <summary>
        /// Invokes the method associated with this Signature.
        /// </summary>
        /// <param name="instance">The instance to invoke on.</param>
        /// <param name="sig">The signature to invoke.</param>
        /// <param name="parameters">The parameters to invoke the method with.</param>
        /// <returns>The return of the invocation.</returns>
        internal static unsafe object InvokeInternal<T>(ref T instance, BaseSignature sig, params object?[] parameters)
        {
            void** ptr = Intrinsics.GetPointerArray(parameters);

            if (sig.IsCtor)
            {
                // I won't add a check for if instance is null, since it will throw anyway.
                instance = (T)_invokeInternal(instance, (nint)ptr, sig.NativeSignature, true);
                Marshal.FreeHGlobal((nint)ptr);
                return instance;
            }

            object ret = _invokeInternal(instance, (nint)ptr, sig.NativeSignature, false);
            Marshal.FreeHGlobal((nint)ptr);
            return ret;
        }

        /// <summary>
        /// Invokes the method associated with this Signature.
        /// </summary>
        /// <remarks>
        /// To invoke on an instance, use <see cref="Invoke{T}(ref T, object[])"/>
        /// </remarks>
        /// <param name="sig">The signature to invoke.</param>
        /// <param name="parameters">The parameters to invoke the method with.</param>
        /// <returns>The return of the invocation.</returns>
        internal static unsafe object InvokeInternal(BaseSignature sig, params object?[] parameters)
        {
            void** ptr = Intrinsics.GetPointerArray(parameters);
            object ret = _invokeInternal(null, (nint)ptr, sig.NativeSignature, sig.IsCtor);
            Marshal.FreeHGlobal((nint)ptr);
            return ret;
        }
    }
}
