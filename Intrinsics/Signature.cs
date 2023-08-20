using System.Reflection;

namespace Cassowary.Intrinsics
{
    public struct Signature
    {
        private object _nativeSignature;

        public Signature(object nativeSignature, bool typeSafetyCheck = true)
        {
            if (!typeSafetyCheck && nativeSignature.GetType() != Type.GetType("System.Signature"))
                throw new ArgumentException("'nativeSignature' argument is not of type System.Signature");

            _nativeSignature = nativeSignature;
        }

        public static Signature GetSignatureSafe(MethodInfo methodInfo)
        {
            object rtMethodInfo = IntrinsicHelpers.AsRuntimeMethodInfo(methodInfo);
            return new Signature(rtMethodInfo.GetType().GetProperty(
                "Signature",
                BindingFlags.NonPublic |
                BindingFlags.Instance)!
                .GetValue(rtMethodInfo)!);
        }

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
    }
}
