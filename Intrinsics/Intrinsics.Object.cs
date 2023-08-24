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

using Cassowary.Intrinsics.VM;
using Cassowary.Intrinsics.VM.Cor;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Cassowary.Intrinsics
{
    public static unsafe partial class Intrinsics
    {
        /// <summary>
        /// Gets the MethodTable for the specified object.
        /// </summary>
        /// <param name="obj">The object to get the MethodTable for.</param>
        /// <returns>The MethodTable for the specified object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodTable* GetMethodTable(object obj)
        {
            return MethodTable.FromObject(obj);
        }

        /// <summary>
        /// Allocates and returns an object of the specified type, checks if <see cref="MethodTable.CanAllocate"/> returns true.
        /// </summary>
        /// <param name="type">The type of the object to be allocated.</param>
        /// <returns>The allocated object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Allocate(Type type)
        {
            return GetMethodTable(type)->Allocate();
        }

        /// <summary>
        /// Allocates and returns an object of the specified type array with the specified lengths, checks if <see cref="MethodTable.CanAllocate"/> returns true.
        /// </summary>
        /// <param name="type">The type of the object to be allocated.</param>
        /// <returns>The allocated object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Allocate(Type type, int length)
        {
            return GetMethodTable(type)->Allocate(length);
        }

        /// <summary>
        /// Allocates and returns an object of the specified type array with the specified lengths, checks if <see cref="MethodTable.CanAllocate"/> returns true.
        /// </summary>
        /// <param name="type">The type of the object to be allocated.</param>
        /// <returns>The allocated object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Allocate(Type type, params int[] lengths)
        {
            return GetMethodTable(type)->Allocate(lengths);
        }

        /// <summary>
        /// Allocates and returns an object of the specified type, does not check if <see cref="MethodTable.CanAllocate"/> returns true.
        /// </summary>
        /// <param name="type">The type of the object to be allocated.</param>
        /// <returns>The allocated object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object AllocateNoChecks(Type type)
        {
            return GetMethodTable(type)->AllocateNoChecks();
        }

        /// <summary>
        /// Casts the given object with the given MethodTable, checks if <see cref="MethodTable.CanCastTo"/> returns true.
        /// </summary>
        /// <param name="obj">The object to cast to the provided MethodTable.</param>
        /// <param name="type">The type to cast the object to.</param>
        /// <returns>Obj as pMT.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Cast(object obj, Type type)
        {
            return GetMethodTable(type)->Cast(obj);
        }

        /// <summary>
        /// Casts the given object with the given MethodTable, does not check if <see cref="MethodTable.CanCastTo"/> returns true.
        /// </summary>
        /// <param name="obj">The object to cast to the provided MethodTable.</param>
        /// <param name="type">The type to cast the object to.</param>
        /// <returns>Obj as pMT.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object CastNoChecks(object obj, Type type)
        {
            return GetMethodTable(type)->CastNoChecks(obj);
        }

        /// <summary>
        /// Boxes a value from a pointer of a specified type.
        /// </summary>
        /// <param name="ptr">A pointer to the value.</param>
        /// <param name="type">The type of the value.</param>
        /// <returns>The boxed object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Box(void* ptr, Type type)
        {
            return GetMethodTable(type)->Box(ptr);
        }

        /// <summary>
        /// Boxes a value from a pointer of a specified type, strictly adhering to boxing rules.
        /// </summary>
        /// <param name="ptr">A pointer to the value.</param>
        /// <param name="type">The type of the value.</param>
        /// <returns>The boxed object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object BoxStrict(void* ptr, Type type)
        {
            return GetMethodTable(type)->BoxStrict(ptr);
        }

        /// <summary>
        /// Constructs on an unboxed object using the default constructor.
        /// </summary>
        /// <param name="instance">The object to be constructed.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Construct<T>(ref T instance)
        {
            GetMethodTable(instance!)->Construct(ref instance);
        }

        /// <summary>
        /// Constructs on an unboxed object with the given parameters.
        /// </summary>
        /// <param name="instance">The object to be constructed.</param>
        /// <param name="parameters">The parameters for the object's constructor.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Construct<T>(ref T instance, params object[] parameters)
        {
            GetMethodTable(instance!)->Construct(ref instance, parameters);
        }

        /// <summary>
        /// Constructs an object of the specified type using the default constructor.
        /// </summary>
        /// <param name="type">The type of the object to be constructed.</param>
        /// <returns>The constructed object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Construct(Type type)
        {
            return GetMethodTable(type)->Construct();
        }

        /// <summary>
        /// Constructs an object of the specified type with the given parameters.
        /// </summary>
        /// <param name="type">The type of the object to be constructed.</param>
        /// <param name="parameters">The parameters for the object's constructor.</param>
        /// <returns>The constructed object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Construct(Type type, params object[] parameters)
        {
            return GetMethodTable(type)->Construct(parameters);
        }

        /// <summary>
        /// Destroys an object, wiping all of its bytes and allowing it to be safely modified or reconstructed.
        /// </summary>
        /// <param name="obj">The object to be destroyed.</param>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static void Destroy(object obj)
        {
            MethodTable* pMT = GetMethodTable(obj);

            if (pMT->IsStringOrArray)
                // Strings and Arrays have length fields at offset 0, overwriting that can have catastrophic effects.
                NativeMemory.Clear(Unsafe.Add(GetPointer(obj), CorDataOffsets.OFFSET_STRING), (nuint)(((Array)obj).Length * pMT->ComponentSize));

            NativeMemory.Clear(GetPointer(obj), (nuint)pMT->GetNumInstanceFieldBytes());
        }

        /// <summary>
        /// Creates a deep clone of the specified object.
        /// </summary>
        /// <param name="obj">The object to clone.</param>
        /// <returns>A cloned object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static object CreateClone(object obj)
        {
            return Box(GetPointer(obj), obj.GetType());
        }

        /// <summary>
        /// Creates an uninitialized clone of the specified object.
        /// </summary>
        /// <param name="obj">The object to clone.</param>
        /// <returns>An uninitialized clone of the object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object CreateUninitializedClone(object obj)
        {
            return GetMethodTable(obj)->Allocate();
        }

        /// <summary>
        /// Gets the heap data for the specified object.
        /// </summary>
        /// <param name="obj">The object to get the heap data for.</param>
        /// <returns>A reference to the heap data for the specified object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref byte GetData(object obj)
        {
            return ref Unsafe.As<RelaxedGrossObject>(obj).Data;
        }

        /// <summary>
        /// Gets the heap pointer for the specified object.
        /// </summary>
        /// <param name="obj">The object to get the heap pointer for.</param>
        /// <returns>The heap pointer for the specified object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* GetPointer(object? obj)
        {
            if (obj == null)
                return null;

            return Unsafe.AsPointer(ref Unsafe.As<RelaxedGrossObject>(obj).Data);
        }

        public static void** GetPointerArray(object?[] objs)
        {
            void** voidArray = (void**)Marshal.AllocHGlobal(objs.Length * sizeof(void*));

            for (int i = 0; i < objs.Length; i++)
                voidArray[i] = GetPointer(objs[i]);

            return voidArray;
        }
    }
}
