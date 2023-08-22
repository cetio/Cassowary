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
        /// <summary>
        /// Reads a value from memory.
        /// </summary>
        /// <typeparam name="T">The type of the value to read.</typeparam>
        /// <param name="source">Pointer to the source memory.</param>
        /// <returns>The read value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Read<T>(void* source)
        {
            Ldarg(nameof(source));
            Ldobj(typeof(T));
            return IL.Return<T>();
        }

        /// <summary>
        /// Writes the value of type T to the specified memory location.
        /// </summary>
        /// <typeparam name="T">The type of value to write.</typeparam>
        /// <param name="destination">The memory location to write to.</param>
        /// <param name="value">The value to write.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write<T>(void* destination, T value)
        {
            Ldarg(nameof(destination));
            Ldarg(nameof(value));
            Stobj(typeof(T));
        }

        /// <summary>
        /// Copies the value of type T to the specified memory location.
        /// </summary>
        /// <typeparam name="T">The type of value to copy.</typeparam>
        /// <param name="destination">The memory location to copy to.</param>
        /// <param name="source">The reference to the source value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy<T>(void* destination, ref T source)
        {
            Ldarg(nameof(destination));
            Ldarg(nameof(source));
            Ldobj(typeof(T));
            Stobj(typeof(T));
        }

        /// <summary>
        /// Copies the value from a memory location to the specified destination reference.
        /// </summary>
        /// <typeparam name="T">The type of value to copy.</typeparam>
        /// <param name="destination">The reference to the destination value.</param>
        /// <param name="source">The memory location of the source value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy<T>(ref T destination, void* source)
        {
            Ldarg(nameof(destination));
            Ldarg(nameof(source));
            Ldobj(typeof(T));
            Stobj(typeof(T));
        }

        /// <summary>
        /// Converts a reference to a value type to a pointer to the value.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The reference to the value.</param>
        /// <returns>A pointer to the value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* AsPointer<T>(ref T value)
        {
            Ldarg(nameof(value));
            Conv_U();
            return IL.ReturnPointer();
        }

        /// <summary>
        /// Copies a specified number of bytes from one memory location to another.
        /// </summary>
        /// <param name="destination">The destination memory location.</param>
        /// <param name="source">The source memory location.</param>
        /// <param name="byteCount">The number of bytes to copy.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyBlock(void* destination, void* source, uint byteCount)
        {
            Ldarg(nameof(destination));
            Ldarg(nameof(source));
            Ldarg(nameof(byteCount));
            Cpblk();
        }

        /// <summary>
        /// Copies a specified number of bytes from one memory location to another using references.
        /// </summary>
        /// <param name="destination">The reference to the destination memory location.</param>
        /// <param name="source">The reference to the source memory location.</param>
        /// <param name="byteCount">The number of bytes to copy.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyBlock(ref byte destination, ref byte source, uint byteCount)
        {
            Ldarg(nameof(destination));
            Ldarg(nameof(source));
            Ldarg(nameof(byteCount));
            Cpblk();
        }

        /// <summary>
        /// Converts an object to the specified type.
        /// </summary>
        /// <typeparam name="T">The target type to convert to.</typeparam>
        /// <param name="o">The object to convert.</param>
        /// <returns>The converted object as the specified type.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T As<T>(object o)
            where T : class
        {
            Ldarg(nameof(o));
            return IL.Return<T>();
        }

        /// <summary>
        /// Returns the object at source as a reference to type ref T.
        /// </summary>
        /// <param name="source">The source pointer to get a reference to T from.</param>
        /// <returns>The object at source as ref T.</returns>
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

        /// <summary>
        /// Returns source as a reference to type ref T.
        /// </summary>
        /// <param name="source">The source object to get a reference to T from.</param>
        /// <returns>Source as ref T.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AsRef<T>(in T source)
        {
            Ldarg(nameof(source));
            return ref IL.ReturnRef<T>();
        }

        /// <summary>
        /// Casts an object as the given type from the given type.
        /// </summary>
        /// <param name="source">The source object to cast as TTo.</param>
        /// <returns>Source as TTo.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TTo As<TFrom, TTo>(ref TFrom source)
        {
            Ldarg(nameof(source));
            return ref IL.ReturnRef<TTo>();
        }

        /// <summary>
        /// Adds an offset from a memory pointer.
        /// </summary>
        /// <param name="source">Pointer to the source memory.</param>
        /// <param name="byteOffset">Offset to add.</param>
        /// <returns>The adjusted memory pointer.</returns>
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

        /// <summary>
        /// Adds an offset from a typed reference and returns the resulting reference.
        /// </summary>
        /// <typeparam name="T">The type of the reference.</typeparam>
        /// <param name="source">The source reference.</param>
        /// <param name="elementOffset">The offset in elements of type T.</param>
        /// <returns>The resulting reference.</returns>
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

        /// <summary>
        /// Subtracts an offset from a memory pointer.
        /// </summary>
        /// <param name="source">Pointer to the source memory.</param>
        /// <param name="byteOffset">Offset to subtract.</param>
        /// <returns>The adjusted memory pointer.</returns>
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

        /// <summary>
        /// Subtracts an offset from a typed reference and returns the resulting reference.
        /// </summary>
        /// <typeparam name="T">The type of the reference.</typeparam>
        /// <param name="source">The source reference.</param>
        /// <param name="elementOffset">The offset in elements of type T.</param>
        /// <returns>The resulting reference.</returns>
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
