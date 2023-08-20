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

using Cassowary.Reflection.Factories;
using System.Reflection;

namespace Cassowary.Reflection
{
    public static class Invoker
    {
        /// <summary>
        /// Invokes the given MethodInfo, this is faster than alternatives built into C# if CreateDelegate is cached.
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns>The return value of the given MethodInfo.</returns>
        public static object? FastInvoke(MethodInfo methodInfo, object? target, params object?[] parameters)
        {
            return FastInvoke(DelegateFactory.MakeDelegate(methodInfo, target), parameters);
        }

        /// <summary>
        /// Invokes the given Delegate, this is faster than alternatives built into C#.
        /// </summary>
        /// <param name="del"></param>
        /// <param name="parameters"></param>
        /// <returns>The return value of the given Delegate.</returns>
        public static object? FastInvoke(Delegate del, params dynamic?[] parameters)
        {
            dynamic @delegate = del;
            object? ret = null;

            if (del.Method.ReturnType == typeof(void))
                return FastInvokeNoReturn(@delegate, parameters);

            switch (parameters.Length)
            {
                case 0:
                    ret = @delegate();
                    break;
                case 1:
                    ret = @delegate(parameters[0]);
                    break;
                case 2:
                    ret = @delegate(parameters[0], parameters[1]);
                    break;
                case 3:
                    ret = @delegate(parameters[0], parameters[1], parameters[2]);
                    break;
                case 4:
                    ret = @delegate(parameters[0], parameters[1], parameters[2], parameters[3]);
                    break;
                case 5:
                    ret = @delegate(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4]);
                    break;
                case 6:
                    ret = @delegate(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5]);
                    break;
                case 7:
                    ret = @delegate(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6]);
                    break;
                case 8:
                    ret = @delegate(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7]);
                    break;
                case 9:
                    ret = @delegate(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7], parameters[8]);
                    break;
                case 10:
                    ret = @delegate(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7], parameters[8], parameters[9]);
                    break;
                case 11:
                    ret = @delegate(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7], parameters[8], parameters[9], parameters[10]);
                    break;
                case 12:
                    ret = @delegate(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7], parameters[8], parameters[9], parameters[10], parameters[11]);
                    break;
                case 13:
                    ret = @delegate(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7], parameters[8], parameters[9], parameters[10], parameters[11], parameters[12], parameters[13]);
                    break;
                default:
                    ret = @delegate.DynamicInvoke(parameters);
                    break;
            }

            return ret;
        }

        private static object? FastInvokeNoReturn(dynamic @delegate, params dynamic?[] parameters)
        {
            switch (parameters.Length)
            {
                case 0:
                    @delegate();
                    break;
                case 1:
                    @delegate(parameters[0]);
                    break;
                case 2:
                    @delegate(parameters[0], parameters[1]);
                    break;
                case 3:
                    @delegate(parameters[0], parameters[1], parameters[2]);
                    break;
                case 4:
                    @delegate(parameters[0], parameters[1], parameters[2], parameters[3]);
                    break;
                case 5:
                    @delegate(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4]);
                    break;
                case 6:
                    @delegate(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5]);
                    break;
                case 7:
                    @delegate(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6]);
                    break;
                case 8:
                    @delegate(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7]);
                    break;
                case 9:
                    @delegate(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7], parameters[8]);
                    break;
                case 10:
                    @delegate(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7], parameters[8], parameters[9]);
                    break;
                case 11:
                    @delegate(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7], parameters[8], parameters[9], parameters[10]);
                    break;
                case 12:
                    @delegate(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7], parameters[8], parameters[9], parameters[10], parameters[11]);
                    break;
                case 13:
                    @delegate(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7], parameters[8], parameters[9], parameters[10], parameters[11], parameters[12], parameters[13]);
                    break;
                default:
                    @delegate.DynamicInvoke(parameters);
                    break;
            }

            return null;
        }
    }
}
