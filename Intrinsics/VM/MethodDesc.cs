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
using JetBrains.Annotations;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Cassowary.Intrinsics.VM
{
    public enum MethodClassification
    {
        /// <summary>
        ///     IL
        /// </summary>
        IL = 0,

        /// <summary>
        ///     FCall(also includes tlbimped ctor, Delegate ctor)
        /// </summary>
        FCall = 1,


        /// <summary>
        ///     N/Direct
        /// </summary>
        NDirect = 2,


        /// <summary>
        ///     Special method; implementation provided by EE (like Delegate Invoke)
        /// </summary>
        EEImpl = 3,

        /// <summary>
        ///     Array ECall
        /// </summary>
        Array = 4,

        /// <summary>
        ///     Instantiated generic methods, including descriptors
        ///     for both shared and unshared code (see InstantiatedMethodDesc)
        /// </summary>
        Instantiated = 5,

        /// <summary>
        ///     For <see cref="MethodDesc" /> with no metadata behind
        /// </summary>
        Dynamic = 7,
        Count
    }

    [Flags]
    public enum MethodDescClassification : ushort
    {
        /// <summary>
        ///     Method is <see cref="MethodClassification.IL" />, <see cref="MethodClassification.FCall" /> etc., see
        ///     <see cref="MethodClassification" /> above.
        /// </summary>
        Classification = 0x0007,
        ClassificationCount = Classification + 1,

        // Note that layout of code:MethodDesc::s_ClassificationSizeTable depends on the exact values
        // of mdcHasNonVtableSlot and mdcMethodImpl

        /// <summary>
        ///     Has local slot (vs. has real slot in MethodTable)
        /// </summary>
        HasNonVtableSlot = 0x0008,

        /// <summary>
        ///     Method is a body for a method impl (MI_MethodDesc, MI_NDirectMethodDesc, etc)
        ///     where the function explicitly implements IInterface.foo() instead of foo().
        /// </summary>
        MethodImpl = 0x0010,

        /// <summary>
        ///     Method is static
        /// </summary>
        Static = 0x0020,

        // Duplicate method. When a method needs to be placed in multiple slots in the
        // method table, because it could not be packed into one slot. For eg, a method
        // providing implementation for two interfaces, MethodImpl, etc
        Duplicate = 0x0400,

        /// <summary>
        ///     Has this method been verified?
        /// </summary>
        VerifiedState = 0x0800,

        /// <summary>
        ///     Is the method verifiable? It needs to be verified first to determine this
        /// </summary>
        Verifiable = 0x1000,

        /// <summary>
        ///     Is this method ineligible for inlining?
        /// </summary>
        NotInline = 0x2000,

        /// <summary>
        ///     Is the method synchronized
        /// </summary>
        Synchronized = 0x4000,

        /// <summary>
        ///     Does the method's slot number require all 16 bits
        /// </summary>
        RequiresFullSlotNumber = 0x8000
    }

    [Flags]
    public enum DescCodeFlags : byte
    {
        /// <summary>
        ///     The method entrypoint is stable (either precode or actual code)
        /// </summary>
        HasStableEntryPoint = 0x01,

        /// <summary>
        ///     implies that HasStableEntryPoint is set.
        ///     Precode has been allocated for this method
        /// </summary>
        HasPrecode = 0x02,

        IsUnboxingStub = 0x04,

        /// <summary>
        ///     Has slot for native code
        /// </summary>
        HasNativeCodeSlot = 0x08,

        /// <summary>
        ///     Jit may expand method as an intrinsic
        /// </summary>
        IsJitIntrinsic = 0x10
    }

    [Flags]
    public enum ParamFlags : ushort
    {
        TokenRemainderMask = 0x3FFF,

        // These are separate to allow the flags space available and used to be obvious here
        // and for the logic that splits the token to be algorithmically generated based on the
        // #define

        /// <summary>
        ///     Indicates that a type-forwarded type is used as a valuetype parameter (this flag is only valid for ngenned items)
        /// </summary>
        HasForwardedValuetypeParameter = 0x4000,

        /// <summary>
        ///     Indicates that all typeref's in the signature of the method have been resolved to typedefs (or that process failed)
        ///     (this flag is only valid for non-ngenned methods)
        /// </summary>
        ValueTypeParametersWalked = 0x4000,

        /// <summary>
        ///     Indicates that we have verified that there are no equivalent valuetype parameters for this method
        /// </summary>
        DoesNotHaveEquivalentValuetypeParameters = 0x8000
    }

    [ShouldUsePointerNotObject]
    [Intrinsic]
    [StructLayout(LayoutKind.Explicit)]
    public readonly unsafe struct MethodDesc
    {
        [FieldOffset(0)]
        public readonly ParamFlags ParamFlags; // m_wFlags3AndTokenRemainder

        [FieldOffset(2)]
        public readonly byte ChunkIndex; // m_chunkIndex

        [FieldOffset(3)]
        public readonly DescCodeFlags CodeFlags; // m_wCodeFlags

        [FieldOffset(4)]
        public readonly ushort SlotNumber; // m_wSlotNumber

        [FieldOffset(6)]
        public readonly MethodDescClassification MethodDescClassification; // m_wFlags

        /// <summary>
        ///     Valid only if the function is non-virtual,
        ///     non-abstract, non-generic (size of this MethodDesc <c>== 16</c>)
        /// </summary>
        [FieldOffset(8)]
        public readonly void* NonVtableSlot;

        /// <summary>
        ///     Valid only if the function is non-virtual,
        ///     non-abstract, non-generic
        /// </summary>
        [FieldOffset(8)]
        public readonly void* NativeCodeSlot;

        [FieldOffset(16)]
        public readonly MethodDesc* DeclaringMethodDesc; // m_pMethodDesc

        [CanBeNull]
        [FieldOffset(24)]
        public readonly NativeCodeVersion NativeCodeVersion; // m_nativeCodeVersion

        [FieldOffset(28)]
        public readonly bool NeedsMulticoreJitNotification; // m_needsMulticoreJitNotification

        [FieldOffset(29)]
        public readonly bool MayUsePrecompiledCode; // m_mayUsePrecompiledCode

        [FieldOffset(30)]
        public readonly bool ProfilerRejectedPrecompiledCode; // m_ProfilerRejectedPrecompiledCode

        [FieldOffset(31)]
        public readonly bool ReadyToRunRejectedPrecompiledCode; // m_ReadyToRunRejectedPrecompiledCode

        public MethodClassification MethodClassification
        {
            get
            {
                return (MethodClassification)((ushort)MethodDescClassification & (ushort)MethodDescClassification.Classification);
            }
        }

        public bool IsIL
        {
            get
            {
                return MethodClassification.HasFlag(MethodClassification.IL);
            }
        }

        public bool IsFCall
        {
            get
            {
                return MethodClassification.HasFlag(MethodClassification.FCall);
            }
        }

        public bool IsNDirect
        {
            get
            {
                return MethodClassification.HasFlag(MethodClassification.NDirect);
            }
        }

        public bool IsEEImpl
        {
            get
            {
                return MethodClassification.HasFlag(MethodClassification.EEImpl);
            }
        }

        public bool IsArray
        {
            get
            {
                return MethodClassification.HasFlag(MethodClassification.Array);
            }
        }

        public bool IsInstantiated
        {
            get
            {
                return MethodClassification.HasFlag(MethodClassification.Instantiated);
            }
        }

        public bool IsDynamic
        {
            get
            {
                return MethodClassification.HasFlag(MethodClassification.Dynamic);
            }
        }

        public bool HasNonVtableSlot
        {
            get
            {
                return MethodDescClassification.HasFlag(MethodDescClassification.HasNonVtableSlot);
            }
        }

        public bool IsMethodImpl
        {
            get
            {
                return MethodDescClassification.HasFlag(MethodDescClassification.MethodImpl);
            }
        }

        public bool IsStatic
        {
            get
            {
                return MethodDescClassification.HasFlag(MethodDescClassification.Static);
            }
        }

        public bool IsDuplicate
        {
            get
            {
                return MethodDescClassification.HasFlag(MethodDescClassification.Duplicate);
            }
        }

        public bool IsVerified
        {
            get
            {
                return MethodDescClassification.HasFlag(MethodDescClassification.VerifiedState);
            }
        }

        public bool IsVerifiable
        {
            get
            {
                return MethodDescClassification.HasFlag(MethodDescClassification.Verifiable);
            }
        }

        public bool IsNotInlined
        {
            get
            {
                return MethodDescClassification.HasFlag(MethodDescClassification.NotInline);
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return MethodDescClassification.HasFlag(MethodDescClassification.Synchronized);
            }
        }

        public bool RequiresFullSlotNumber 
        {
            get
            {
                return MethodDescClassification.HasFlag(MethodDescClassification.RequiresFullSlotNumber);
            }
        }

        public bool HasStableEntryPoint
        {
            get
            {
                return CodeFlags.HasFlag(DescCodeFlags.HasStableEntryPoint);
            }
        }

        public bool HasPrecode
        {
            get
            {
                return CodeFlags.HasFlag(DescCodeFlags.HasPrecode);
            }
        }

        public bool IsUnboxingStub
        {
            get
            {
                return CodeFlags.HasFlag(DescCodeFlags.IsUnboxingStub);
            }
        }

        public bool HasNativeCodeSlot
        {
            get
            {
                return CodeFlags.HasFlag(DescCodeFlags.HasNativeCodeSlot);
            }
        }

        public bool IsJitIntrinsic
        {
            get
            {
                return CodeFlags.HasFlag(DescCodeFlags.IsJitIntrinsic);
            }
        }

        public bool HasForwardedValuetypeParameter
        {
            get
            {
                return ParamFlags.HasFlag(ParamFlags.HasForwardedValuetypeParameter);
            }
        }

        public bool ValueTypeParametersWalked
        {
            get
            {
                return ParamFlags.HasFlag(ParamFlags.ValueTypeParametersWalked);
            }
        }

        public bool DoesNotHaveEquivalentValuetypeParameters
        {
            get
            {
                return ParamFlags.HasFlag(ParamFlags.DoesNotHaveEquivalentValuetypeParameters);
            }
        }

        internal RuntimeMethodHandle RuntimeMethodHandle
        {
            get
            {
                fixed (MethodDesc* ptr = &this)
                {
                    return RuntimeMethodHandle.FromIntPtr((nint)ptr);
                }

            }
        }

        public bool IsConstructor => MethodBase.GetMethodFromHandle(RuntimeMethodHandle) is ConstructorInfo;

        public string Name
        {
            get
            {
                if (IsConstructor)
                    return ".ctor";

                MethodInfo methodInfo = (MethodInfo)MethodBase.GetMethodFromHandle(RuntimeMethodHandle);
                dynamic rtMethodInfo = Intrinsics.AsRuntimeMethodInfo(methodInfo);

                return rtMethodInfo.Name;
            }
        }
    }
}