using Cassowary.Factories;
using Cassowary.Intrinsics.VM.Cor;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Cassowary.Intrinsics.VM
{
    public unsafe partial struct MethodTable
    {
        // The types nor methods should ever be null, unless they're removed.
        private static Func<nint, object> _allocateInternal = 
            Unsafe.As<Func<nint, object>>(DelegateFactory.MakeDelegate(Type.GetType("System.StubHelpers.StubHelpers")!
            .GetMethod("AllocateInternal", BindingFlags.NonPublic | BindingFlags.Static)!));

        private static Func<nint, int, int, Array> _allocateSZArrayInternal = 
            Unsafe.As<Func<nint, int, int, Array>>(DelegateFactory.MakeDelegate(typeof(GC)
            .GetMethod("AllocateNewArray", BindingFlags.NonPublic | BindingFlags.Static)!));

        /// <summary>
        /// Allocates this MethodTable, checks if <see cref="MethodTable.CanAllocate"/> returns true.
        /// </summary>
        /// <returns>The allocated object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Allocate()
        {
            if (!CanAllocate())
                throw new ArgumentException($"{this} does not support allocation."); ;

            // This is okay because we already checked if we can allocate.
            return AllocateNoChecks();
        }

        /// <summary>
        /// Allocates this MethodTable, if it is an multi-dimensional Array.
        /// </summary>
        /// <param name="lengths">The lengths of the Array to allocate.</param>
        /// <returns>The allocated Array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public Array Allocate(params int[] lengths)
        {
            if (!IsArray)
                throw new ArgumentException($"{this} is not an array type, and cannot allocate using Allocate(params int[])");

            if (IsSZArray)
                return Allocate(lengths[0]);

            return Array.CreateInstance(ElementMethodTable->AsType(), lengths);
        }

        /// <summary>
        /// Allocates this MethodTable, if it is a single-dimensional Array.
        /// </summary>
        /// <param name="length">The length of the Array to allocate.</param>
        /// <returns>The allocated Array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public object[] Allocate(int length)
        {
            if (!IsSZArray)
                throw new ArgumentException($"{this} is not an single-dimensional array type, and cannot allocate using Allocate(int)");

            return Unsafe.As<object[]>(_allocateSZArrayInternal((nint)ElementMethodTable, length, 16));
        }

        /// <summary>
        /// Allocates and returns an object of the specified type, does not check if <see cref="MethodTable.CanAllocate"/> returns true.
        /// </summary>
        /// <returns>The allocated object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public object AllocateNoChecks()
        {
            if (IsString)
                return string.Empty;

            if (IsSZArray)
                return Allocate(1);

            if (IsArray)
            {
                int[] bounds = new int[GetClass()->AsArrayClass()->Rank];

                for (int i = 0; i < GetClass()->AsArrayClass()->Rank; i++)
                    bounds[i] = 1;

                return Allocate(bounds);
            }

            fixed (MethodTable* ptr = &this)
            {
                return _allocateInternal((nint)ptr);
            }
        }

        /// <summary>
        /// Boxes a value from a pointer of a specified type, checks if <see cref="CanBox"/> returns true.
        /// </summary>
        /// <param name="ptr">A pointer to the value.</param>
        /// <returns>The boxed object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public object Box(void* ptr)
        {
            if (!CanBox())
                throw new ArgumentException($"{this} does not support boxing.");

            // This is okay because we already checked if the boxing is allowed.
            return BoxNoChecks(ptr);
        }

        /// <summary>
        /// Strictly boxes a value from a pointer of a specified type, checks if <see cref="CanBoxStrict"/> returns true.
        /// </summary>
        /// <param name="ptr">A pointer to the value.</param>
        /// <returns>The boxed object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public object BoxStrict(void* ptr)
        {
            if (!CanBoxStrict())
                throw new ArgumentException($"{this} does not support strict boxing.");

            // This is okay because we already checked if the boxing is allowed.
            return BoxNoChecks(ptr);
        }

        /// <summary>
        /// Boxes a value from a pointer of a specified type, does not check if <see cref="CanBox"/> or <see cref="CanBoxStrict"/> returns true.
        /// </summary>
        /// <param name="ptr">A pointer to the value.</param>
        /// <returns>The boxed object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public object BoxNoChecks(void* ptr)
        {
            // ok, maybe some checks
            if (IsString)
                return new string((char*)Unsafe.Add(ptr, CorDataOffsets.OFFSET_STRING), 0, *(int*)ptr);

            if (IsSZArray)
            {
                Array array = Allocate(*(int*)ptr);
                Unsafe.CopyBlock(Intrinsics.GetPointer(array), ptr, (uint)ComponentSize + CorDataOffsets.OFFSET_ARRAY);
                return array;
            }

            if (IsArray)
            {
                int[] bounds = new int[GetClass()->AsArrayClass()->Rank];
                int bytes = 8;

                for (int i = 0; i < GetClass()->AsArrayClass()->Rank; i++)
                {
                    bounds[i] = *(int*)ptr;
                    bytes += *(int*)ptr * ComponentSize;
                }

                Array array = Allocate(bounds);
                Unsafe.CopyBlock(Intrinsics.GetPointer(array), ptr, (uint)bytes);
                return array;
            }

            object obj = AllocateNoChecks();
            Unsafe.Copy(ref Intrinsics.GetData(obj), ptr);
            return obj;
        }

        /// <summary>
        /// Casts the given object to this MethodTable, checks if <see cref="MethodTable.CanCastTo"/> returns true.
        /// </summary>
        /// <param name="obj">The object to cast to the provided MethodTable.</param>
        /// <returns>Obj as pMT.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public object Cast(object obj)
        {
            fixed (MethodTable* pMT = &this)
            {
                MethodTable* poMT = FromObject(obj);

                if (poMT == pMT)
                    return obj;

                if (!poMT->CanCastTo(pMT))
                    throw new ArgumentException($"{*poMT} cannot cast to {*pMT}");

                // This is okay because we already checked if the cast can happen.
                return CastNoChecks(obj);
            }
        }

        /// <summary>
        /// Casts the given object to this MethodTable, does not check if <see cref="MethodTable.CanCastTo"/> returns true.
        /// </summary>
        /// <param name="obj">The object to cast to the provided MethodTable.</param>
        /// <returns>Obj as pMT.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public object CastNoChecks(object obj)
        {
            fixed (MethodTable* pMT = &this)
            {
                // Copy should never return as null, so this is fine.
                object copy = null!;

                if (pMT->IsString)
                {
                    return obj.ToString() ?? string.Empty;
                }
                else if (pMT->IsArray)
                {
                    MethodTable* poMT = FromObject(obj);

                    if (poMT->IsArray)
                        return pMT->BoxNoChecks(Intrinsics.GetPointer(obj));

                    copy = pMT->Allocate(1);
                    Unsafe.CopyBlock(Unsafe.Add(Intrinsics.GetPointer(copy), 8), Unsafe.Add(Intrinsics.GetPointer(obj), 8), (uint)ComponentSize);
                    return copy;
                }
                else
                {
                    copy = pMT->AllocateNoChecks();
                    Unsafe.CopyBlock(ref Intrinsics.GetData(copy), ref Intrinsics.GetData(obj), (uint)pMT->GetNumInstanceFieldBytes());
                    return copy;
                }
            }
        }
    }
}
