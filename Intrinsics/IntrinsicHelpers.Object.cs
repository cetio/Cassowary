using Cassowary.Intrinsics.VM;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Cassowary.Intrinsics
{
    public static unsafe partial class IntrinsicHelpers
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
        /// Gets the underlying MethodTable for the provided pointer.
        /// </summary>
        /// <param name="ptr">The pointer to get the MethodTable from.</param>
        /// <returns>The MethodTable for the specified object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodTable* GetMethodTable(void* ptr)
        {
            return (MethodTable*)Unsafe.Sub(ptr, 8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? Allocate(Type type)
        {
            return GetMethodTable(type)->Allocate();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object AllocateNoChecks(Type type)
        {
            return GetMethodTable(type)->AllocateNoChecks();
        }

        /// <summary>
        /// Casts the given object with the given MethodTable, checks if <see cref="MethodTable.CanCastTo"/> returns true.
        /// </summary>
        /// <param name="obj">The object to cast to the provided MethodTable.</param>
        /// <param name="pMT">The MethodTable used to cast to.</param>
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
        /// <param name="pMT">The MethodTable used to cast to.</param>
        /// <returns>Obj as pMT.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object CastNoChecks(object obj, MethodTable* pMT)
        {
            return pMT->CastNoChecks(obj);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Box(void* ptr, Type type)
        {
            MethodTable* pMT = MethodTable.FromType(type);
            return pMT->Box(ptr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object BoxStrict(void* ptr, Type type)
        {
            MethodTable* pMT = MethodTable.FromType(type);
            return pMT->BoxStrict(ptr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object BoxNoChecks(void* ptr, Type type)
        {
            MethodTable* pMT = MethodTable.FromType(type);
            return pMT->BoxNoChecks(ptr);
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
        public static void* GetPointer(object obj)
        {
            return Unsafe.AsPointer(ref Unsafe.As<RelaxedGrossObject>(obj).Data);
        }

        public static void** GetPointerArray(object[] objs)
        {
            void** voidArray = (void**)Marshal.AllocHGlobal(objs.Length * sizeof(void*));

            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i] == null)
                    throw new ArgumentNullException($"Object at index {i} is null.");

                void* objPtr = (void*)GCHandle.Alloc(objs[i]).AddrOfPinnedObject();
                voidArray[i] = objPtr;
            }

            return voidArray;
        }
    }
}
