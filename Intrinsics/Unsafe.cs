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

using InlineIL;
using System.Runtime.CompilerServices;
using static InlineIL.IL.Emit;

namespace Cassowary.Intrinsics
{
    public static unsafe class Unsafe
    {
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static object CreateClone(object obj)
        {
            throw new NotImplementedException();
            //object clone = Intrinsics.AllocateUninitializedClone(obj);
            //CopyBlock(ref Intrinsics.GetHeapData(clone), ref Intrinsics.GetHeapData(obj), (uint)Intrinsics.GetRawObjectDataSize(obj));
            //return clone;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object CreateUninitializedObject(Type type)
        {
            return Intrinsics.GetMethodTable(type)->Allocate();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object CreateUninitializedClone(object obj)
        {
            throw new NotImplementedException();
            //return Intrinsics.AllocateUninitializedClone(obj);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Read<T>(void* source)
        {
            Ldarg(nameof(source));
            Ldobj(typeof(T));
            return IL.Return<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadUnaligned<T>(void* source)
        {
            Ldarg(nameof(source));
            Unaligned(1);
            Ldobj(typeof(T));
            return IL.Return<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadUnaligned<T>(ref byte source)
        {
            Ldarg(nameof(source));
            Unaligned(1);
            Ldobj(typeof(T));
            return IL.Return<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write<T>(void* destination, T value)
        {
            Ldarg(nameof(destination));
            Ldarg(nameof(value));
            Stobj(typeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUnaligned<T>(void* destination, T value)
        {
            Ldarg(nameof(destination));
            Ldarg(nameof(value));
            Unaligned(1);
            Stobj(typeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUnaligned<T>(ref byte destination, T value)
        {
            Ldarg(nameof(destination));
            Ldarg(nameof(value));
            Unaligned(1);
            Stobj(typeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy<T>(void* destination, ref T source)
        {
            Ldarg(nameof(destination));
            Ldarg(nameof(source));
            Ldobj(typeof(T));
            Stobj(typeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy<T>(ref T destination, void* source)
        {
            Ldarg(nameof(destination));
            Ldarg(nameof(source));
            Ldobj(typeof(T));
            Stobj(typeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* AsPointer<T>(ref T value)
        {
            Ldarg(nameof(value));
            Conv_U();
            return IL.ReturnPointer();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyBlock(void* destination, void* source, uint byteCount)
        {
            Ldarg(nameof(destination));
            Ldarg(nameof(source));
            Ldarg(nameof(byteCount));
            Cpblk();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyBlock(ref byte destination, ref byte source, uint byteCount)
        {
            Ldarg(nameof(destination));
            Ldarg(nameof(source));
            Ldarg(nameof(byteCount));
            Cpblk();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyBlockUnaligned(void* destination, void* source, uint byteCount)
        {
            Ldarg(nameof(destination));
            Ldarg(nameof(source));
            Ldarg(nameof(byteCount));
            Unaligned(1);
            Cpblk();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyBlockUnaligned(ref byte destination, ref byte source, uint byteCount)
        {
            Ldarg(nameof(destination));
            Ldarg(nameof(source));
            Ldarg(nameof(byteCount));
            Unaligned(1);
            Cpblk();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T As<T>(object o)
            where T : class
        {
            Ldarg(nameof(o));
            return IL.Return<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AsRef<T>(void* source)
        {
            // For .NET Core the roundtrip via a local is no longer needed
#if NETCOREAPP
            IL.Push(source);
            return ref IL.ReturnRef<T>();
#else
            // Roundtrip via a local to avoid type mismatch on return that the JIT inliner chokes on.
            IL.DeclareLocals(
                false,
                new LocalVar("local", typeof(int).MakeByRefType())
            );

            IL.Push(source);
            Stloc("local");
            Ldloc("local");
            return ref IL.ReturnRef<T>();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AsRef<T>(in T source)
        {
            Ldarg(nameof(source));
            return ref IL.ReturnRef<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TTo As<TFrom, TTo>(ref TFrom source)
        {
            Ldarg(nameof(source));
            return ref IL.ReturnRef<TTo>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* Add(void* source, int byteOffset)
        {
            Ldarg(nameof(source));
            Ldarg(nameof(byteOffset));
            Sizeof(typeof(byte));
            Conv_I();
            Mul();
            IL.Emit.Add();
            return IL.ReturnPointer();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Add<T>(ref T source, int elementOffset)
        {
            Ldarg(nameof(source));
            Ldarg(nameof(elementOffset));
            Sizeof(typeof(T));
            Conv_I();
            Mul();
            IL.Emit.Add();
            return ref IL.ReturnRef<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* Sub(void* source, int byteOffset)
        {
            Ldarg(nameof(source));
            Ldarg(nameof(byteOffset));
            Sizeof(typeof(byte));
            Conv_I();
            Mul();
            IL.Emit.Sub();
            return IL.ReturnPointer();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Sub<T>(ref T source, int elementOffset)
        {
            Ldarg(nameof(source));
            Ldarg(nameof(elementOffset));
            Sizeof(typeof(T));
            Conv_I();
            Mul();
            IL.Emit.Sub();
            return ref IL.ReturnRef<T>();
        }
    }
}
