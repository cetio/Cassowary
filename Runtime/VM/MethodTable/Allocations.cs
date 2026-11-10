using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Cassowary.Runtime.VM
{
    public unsafe partial struct MethodTable
    {
        // Cached typed readers used by the boxing path. Reflection-created once per type,
        // the delegates only read a typed value from a caller-owned pointer and box it.
        private static readonly ConcurrentDictionary<Type, Func<nint, object?>> s_boxReaders = new();

        private static readonly MethodInfo s_boxCore =
            typeof(MethodTable).GetMethod(nameof(BoxCore), BindingFlags.NonPublic | BindingFlags.Static)!;

        private static object? BoxCore<T>(nint ptr)
        {
            // Reads the complete typed value and applies the C# boxing rules, including
            // Nullable<T> semantics (empty -> null, populated -> boxed T).
            return (object?)System.Runtime.CompilerServices.Unsafe.Read<T>((void*)ptr);
        }

        private static Func<nint, object?> GetBoxReader(Type type)
        {
            return s_boxReaders.GetOrAdd(type,
                t => s_boxCore.MakeGenericMethod(t).CreateDelegate<Func<nint, object?>>());
        }

        private void EnsureBoxable()
        {
            if (!IsBoxableType(AsType()))
            {
                throw new ArgumentException(
                    $"{AsType()} cannot be boxed from a raw pointer: only closed, non-byref-like " +
                    "value types without GC references are supported.");
            }
        }

        /// <summary>
        ///     Allocates this MethodTable, checking managed allocatability rules.
        /// </summary>
        /// <returns>The allocated object.</returns>
        /// <exception cref="ArgumentException">The type cannot be instantiated by supported managed APIs.</exception>
        public object Allocate()
        {
            Type type = AsType();

            if (!IsAllocatableType(type))
                throw new ArgumentException($"{type} does not support allocation.");

            return AllocateNoChecks();
        }

        /// <summary>
        ///     Allocates this MethodTable, if it is a multi-dimensional Array, preserving
        ///     rank and zero lower bounds.
        /// </summary>
        /// <param name="lengths">The lengths of the Array to allocate.</param>
        public object Allocate(params int[] lengths)
        {
            Type type = AsType();

            if (!type.IsArray)
                throw new ArgumentException($"{type} is not an array type, and cannot allocate using Allocate(params int[])");

            int rank = type.GetArrayRank();
            if (lengths.Length != rank)
                throw new ArgumentException($"Expected {rank} lengths for rank-{rank} array {type}, got {lengths.Length}.");

            Array array = Array.CreateInstance(type.GetElementType()!, lengths);

            // A rank-1 non-SZ array type (T[*]) cannot be produced by the SZArray overload,
            // verify the requested shape survived.
            if (array.GetType() != type)
                throw new NotSupportedException($"Cannot preserve exact array type {type} with supported allocation APIs.");

            return array;
        }

        /// <summary>
        ///     Allocates this MethodTable, if it is a single-dimensional Array.
        /// </summary>
        /// <param name="length">The length of the Array to allocate.</param>
        public object Allocate(int length)
        {
            return Allocate(new[] { length });
        }

        /// <summary>
        ///     Allocates and returns an object of the specified type using supported managed
        ///     APIs. Strings produce <see cref="string.Empty"/>, arrays produce a zero-length
        ///     (every dimension zero) instance, everything else uses
        ///     <see cref="RuntimeHelpers.GetUninitializedObject"/>.
        /// </summary>
        /// <returns>The allocated object.</returns>
        public object AllocateNoChecks()
        {
            Type type = AsType();

            if (type == typeof(string))
                return string.Empty;

            if (type.IsArray)
                return Allocate(new int[type.GetArrayRank()]);

            if (!IsAllocatableType(type))
                throw new ArgumentException($"{type} does not support allocation.");

            return RuntimeHelpers.GetUninitializedObject(type);
        }

        /// <summary>
        ///     Boxes a value from a caller-owned pointer to a value of this type.
        /// </summary>
        /// <remarks>
        ///     Supported: closed, non-byref-like value types without GC references (including
        ///     Nullable&lt;T&gt;). The caller owns and must keep the source pointer valid for
        ///     the duration of the call, an arbitrary void* cannot be validated.
        /// </remarks>
        /// <param name="ptr">A pointer to the value.</param>
        /// <returns>The boxed object (null for an empty Nullable&lt;T&gt;).</returns>
        /// <exception cref="ArgumentNullException">ptr is null.</exception>
        /// <exception cref="ArgumentException">The type is not a supported value type.</exception>
        public object? Box(void* ptr)
        {
            EnsureBoxable();
            return BoxNoChecks(ptr);
        }

        /// <summary>
        ///     Strictly boxes a value from a pointer, subject to the same restrictions as
        ///     <see cref="Box"/>: no reference types, arrays, strings, byref-likes, or
        ///     reference-containing structs.
        /// </summary>
        /// <param name="ptr">A pointer to the value.</param>
        /// <returns>The boxed object (null for an empty Nullable&lt;T&gt;).</returns>
        public object? BoxStrict(void* ptr)
        {
            EnsureBoxable();
            return BoxNoChecks(ptr);
        }

        /// <summary>
        ///     Boxes a value from a caller-owned pointer. Despite the name, the same type
        ///     restrictions as <see cref="Box"/> apply, there is no unchecked bypass.
        /// </summary>
        /// <param name="ptr">A pointer to the value, must remain valid for the call.</param>
        /// <returns>The boxed object (null for an empty Nullable&lt;T&gt;).</returns>
        /// <exception cref="ArgumentNullException">ptr is null.</exception>
        /// <exception cref="ArgumentException">The type is not a supported value type.</exception>
        public object? BoxNoChecks(void* ptr)
        {
            ArgumentNullException.ThrowIfNull(ptr);
            EnsureBoxable();
            return GetBoxReader(AsType())((nint)ptr);
        }

        /// <summary>
        ///     Casts the given object to this MethodTable, preserving object identity using
        ///     the runtime's own cast rules.
        /// </summary>
        /// <param name="obj">The object to cast.</param>
        /// <returns>Obj itself, if castable.</returns>
        /// <exception cref="InvalidCastException">The object is not assignable to this type.</exception>
        public object? Cast(object? obj)
        {
            if (obj == null)
                return null;

            Type type = AsType();
            if (!type.IsInstanceOfType(obj))
                throw new InvalidCastException($"Object of type {obj.GetType()} cannot be cast to {type}.");

            return obj;
        }

        /// <summary>
        ///     Casts the given object to this MethodTable. Identical to <see cref="Cast"/>,
        ///     there is no unchecked object-copying path.
        /// </summary>
        public object? CastNoChecks(object? obj)
        {
            return Cast(obj);
        }
    }
}
