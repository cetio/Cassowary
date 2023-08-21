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
            object rtMethodInfo = Intrinsics.AsRuntimeMethodInfo(methodInfo);
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
