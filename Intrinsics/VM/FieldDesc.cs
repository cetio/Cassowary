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

using Cassowary.Attributes;
using Cassowary.Intrinsics.VM.Cor;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Cassowary.Intrinsics.VM
{
    public enum ProtectionAttribute
    {
        Unknown = 0,
        Private = 1,
        PrivateProtected = 2,
        Internal = 3,
        Protected = 4,
        ProtectedInternal = 5,
        Public = 6
    }

    [ShouldUsePointerNotObject]
    [Intrinsic]
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct FieldDesc
    {
        [FieldOffset(0)]
        public readonly MethodTable* ApproxDeclaringMethodTable; // m_pMTOfEnclosingClass

        /// <summary>
        /// <list type="bullet">
        ///   <item><description>unsigned m_mb : 24;</description></item>
        ///   <item><description>unsigned m_isStatic : 1;</description></item>
        ///   <item><description>unsigned m_isThreadLocal : 1;</description></item>
        ///   <item><description>unsigned m_isRVA : 1;</description></item>
        ///   <item><description>unsigned m_prot : 3;</description></item>
        /// </list>
        /// </summary>
        [FieldOffset(8)]
        private readonly uint MetadataBitfield;

        /// <summary>
        /// <list type="bullet">
        ///     <item><description>unsigned m_dwOffset : 27;</description></item>
        ///     <item><description>unsigned m_type : 5;</description></item>
        /// </list>
        /// </summary>
        [FieldOffset(12)]
        private readonly uint LayoutBitfield;

        public uint MB // m_mb
        {
            get
            {
                return MetadataBitfield & ((1 << 24) - 1);
            }
        }

        /// <summary>
        /// Is this field static?
        /// </summary>
        public bool IsStatic // m_isStatic
        {
            get
            {
                return ((MetadataBitfield >> 24) & ((1 << 1) - 1)) == 1;
            }
        }

        /// <summary>
        /// Is this field special static on its thread?
        /// </summary>
        public bool IsThreadLocal // m_isThreadLocal
        {
            get
            {
                return ((MetadataBitfield >> 25) & ((1 << 1) - 1)) == 1;
            }
        }

        /// <summary>
        /// Is this field a PE RVA field?
        /// </summary>
        public bool IsRVA // m_isRVA
        {
            get
            {
                return ((MetadataBitfield >> 26) & ((1 << 1) - 1)) == 1;
            }
        }

        /// <summary>
        /// Protection of this field.
        /// </summary>
        public ProtectionAttribute ProtectionAttribute // m_prot
        {
            get
            {
                return (ProtectionAttribute)((MetadataBitfield >> 27) & ((1 << 3) - 1));
            }
        }

        /// <summary>
        /// Offset is a byte offset from an instance pointer if !IsStatic, otherwise it is an RVA field offset.
        /// </summary>
        public uint Offset // m_dwOffset
        {
            get
            {
                return LayoutBitfield & ((1 << 27) - 1);
            }
        }

        // Temporary values stored in FieldDesc m_dwOffset during loading
        // The high 5 bits must be zero (because in field.h we steal them for other uses), so we must choose values > 0
        internal const int FIELD_OFFSET_MAX = (1 << 27) - 1;
        internal const int FIELD_OFFSET_UNPLACED = FIELD_OFFSET_MAX;
        internal const int FIELD_OFFSET_UNPLACED_GC_PTR = FIELD_OFFSET_MAX - 1;
        internal const int FIELD_OFFSET_VALUE_CLASS = FIELD_OFFSET_MAX - 2;
        internal const int FIELD_OFFSET_NOT_REAL_FIELD = FIELD_OFFSET_MAX - 3;

        // Offset to indicate an EnC added field. They don't have offsets as aren't placed in the object.
        internal const int FIELD_OFFSET_NEW_ENC = FIELD_OFFSET_MAX - 4;
        internal const int FIELD_OFFSET_BIG_RVA = FIELD_OFFSET_MAX - 5;
        internal const int FIELD_OFFSET_LAST_REAL_OFFSET = FIELD_OFFSET_MAX - 6;    // real fields have to be smaller than this

        /// <summary>
        /// The CorElementType of this FieldDesc.
        /// </summary>
        public CorElementType CorElementType // m_type
        {
            get
            {
                return (CorElementType)((LayoutBitfield >> 27) & ((1 << 5) - 1));
            }
        }

        /// <summary>
        /// The CorTokenType of this FieldDesc.
        /// </summary>
        public CorTokenType CorTokenType
        {
            get
            {
                return (CorTokenType)(MB | (int)CorTokenType.mdtFieldDef);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not this FieldDesc is private.
        /// </summary>
        public bool IsPrivate
        {
            get
            {
                return ProtectionAttribute.HasFlag(ProtectionAttribute.Private);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not this FieldDesc is private protected.
        /// </summary>
        public bool IsPrivateProtected
        {
            get
            {
                return ProtectionAttribute.HasFlag(ProtectionAttribute.PrivateProtected);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not this FieldDesc is internal.
        /// </summary>
        public bool IsInternal
        {
            get
            {
                return ProtectionAttribute.HasFlag(ProtectionAttribute.Internal);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not this FieldDesc is protected.
        /// </summary>
        public bool IsProtected
        {
            get
            {
                return ProtectionAttribute.HasFlag(ProtectionAttribute.Protected);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not this FieldDesc is protected internal.
        /// </summary>
        public bool IsProtectedInternal
        {
            get
            {
                return ProtectionAttribute.HasFlag(ProtectionAttribute.ProtectedInternal);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not this FieldDesc is public.
        /// </summary>
        public bool IsPublic
        {
            get
            {
                return ProtectionAttribute.HasFlag(ProtectionAttribute.Public);
            }
        }

        /// <summary>
        /// Gets a value determining whether or not this FieldDesc is refers to an unplaced field.
        /// </summary>
        public bool IsUnplaced
        {
            get
            {
                return Offset == FIELD_OFFSET_UNPLACED;
            }
        }

        /// <summary>
        /// Gets a value determining whether or not this FieldDesc is refers to an unplaced GC Pointer.
        /// </summary>
        public bool IsUnplacedGCPTR
        {
            get
            {
                return Offset == FIELD_OFFSET_UNPLACED_GC_PTR;
            }
        }

        /// <summary>
        /// Gets a value determining whether or not this FieldDesc is refers to an EnC New.
        /// </summary>
        public bool IsEnCNew
        {
            get
            {
                return Offset == FIELD_OFFSET_NEW_ENC;
            }
        }

        /// <summary>
        /// Gets a value determining whether or not this FieldDesc is refers to a Big Relative Value Address.
        /// </summary>
        public bool IsBigRVA
        {
            get
            {
                return Offset == FIELD_OFFSET_BIG_RVA;
            }
        }

        internal RuntimeFieldHandle RuntimeFieldHandle
        {
            get
            {
                fixed (FieldDesc* ptr = &this)
                {
                    return RuntimeFieldHandle.FromIntPtr((nint)ptr);
                }

            }
        }

        public MethodTable* RootCanonDeclaringMethodTable
        {
            get
            {
                return ApproxDeclaringMethodTable->GetRootCanonTable();
            }
        }

        public Type ApproxDeclaringType
        {
            get
            {
                return ApproxDeclaringMethodTable->AsType();
            }
        }

        public Type DeclaringType
        {
            get
            {
                return RootCanonDeclaringMethodTable->AsType();
            }
        }

        public Type FieldType
        {
            get
            {
                FieldInfo fieldInfo = FieldInfo.GetFieldFromHandle(RuntimeFieldHandle);
                dynamic rtFieldInfo = Intrinsics.AsRuntimeFieldInfo(fieldInfo);

                return rtFieldInfo.FieldType;
            }
        }

        public string Name
        {
            get
            {
                FieldInfo fieldInfo = FieldInfo.GetFieldFromHandle(RuntimeFieldHandle);
                dynamic rtFieldInfo = Intrinsics.AsRuntimeFieldInfo(fieldInfo);

                return rtFieldInfo.Name;
            }
        }

        public FieldAttributes FieldAttributes
        {
            get
            {
                FieldInfo fieldInfo = FieldInfo.GetFieldFromHandle(RuntimeFieldHandle);
                dynamic rtFieldInfo = Intrinsics.AsRuntimeFieldInfo(fieldInfo);

                return rtFieldInfo.Attributes;
            }
        }

        public int MetadataToken
        {
            get
            {
                FieldInfo fieldInfo = FieldInfo.GetFieldFromHandle(RuntimeFieldHandle);
                dynamic rtFieldInfo = Intrinsics.AsRuntimeFieldInfo(fieldInfo);

                return rtFieldInfo.MetadataToken;
            }
        }

        public int ManagedSize
        {
            get
            {
                return RootCanonDeclaringMethodTable->GetClass()->ManagedSize;
            }
        }

        /// <summary>
        /// Gets the address of this field, using a pointer to an instance.
        /// </summary>
        /// <param name="ptr">Pointer to an instance to get the field address from.</param>
        /// <returns>The address of this field, following from the given pointer.</returns>
        public void* GetAddress(void* ptr)
        {
            if (IsStatic)
                return Intrinsics.GetPointer(GetValueSafe());

            return Unsafe.Add(ptr, (int)Offset);
        }

        /// <summary>
        /// Gets the value of this field using MethodTable boxing.
        /// </summary>
        /// <param name="obj">An instance to get the field value from.</param>
        /// <returns>The value of this field.</returns>
        public object GetValue(object obj)
        {
            return MethodTable.FromType(FieldType)->BoxNoChecks(GetAddress(Intrinsics.GetPointer(obj)));
        }

        /// <summary>
        /// Gets the value of this field using MethodTable boxing.
        /// </summary>
        /// <param name="ptr">Pointer to an instance to get the field value from.</param>
        /// <returns>The value of this field.</returns>
        public object GetValue(void* ptr)
        {
            return MethodTable.FromType(FieldType)->BoxNoChecks(GetAddress(ptr));
        }

        /// <summary>
        /// Gets the value of this field, without using an instance (static field.)
        /// </summary>
        /// <returns>The value of this field.</returns>
        public object GetValue()
        {
            return GetValueSafe();
        }

        /// <summary>
        /// This is slower than <see cref="GetValue(object)"/> but does not use MethodTable boxing.
        /// </summary>
        /// <param name="obj">The instance to get the field value from.</param>
        /// <returns>The value of this field.</returns>
        public object GetValueSafe(object? obj)
        {
            FieldInfo fieldInfo = FieldInfo.GetFieldFromHandle(RuntimeFieldHandle);

            if (obj == null)
            {
                dynamic rtFieldInfo = Intrinsics.AsRuntimeFieldInfo(fieldInfo);
                return rtFieldInfo.GetValue(null);
            }

            TypedReference typedReference = __makeref(obj);
            return fieldInfo.GetValueDirect(typedReference);
        }

        /// <summary>
        /// Gets the value of this field, without using an instance (static field.)
        /// </summary>
        /// <returns>The value of this field.</returns>
        public object GetValueSafe()
        {
            return GetValueSafe(null);
        }

        public static bool operator ==(FieldDesc fieldDesc, object obj)
        {
            if (obj is not FieldDesc)
                return false;

            return fieldDesc.Equals(obj);
        }

        public static bool operator !=(FieldDesc fieldDesc, object obj)
        {
            if (obj is not FieldDesc)
                return true;

            return !fieldDesc.Equals(obj);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}