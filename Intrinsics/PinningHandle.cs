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

using JetBrains.Annotations;

namespace Cassowary.Intrinsics
{
    public unsafe class PinningHandle
    {
        private ManualResetEvent _manualResetEvent;
        private object _target;

        /// <summary>
        /// Initializes a new instance of the PinningHandle class.
        /// </summary>
        /// <param name="obj">The object to be pinned.</param>
        internal PinningHandle([NotNull] object obj)
        {
            _manualResetEvent = new ManualResetEvent(false);
            _target = obj;

            // Queue a user work item to pin the object in memory.
            ThreadPool.QueueUserWorkItem(_ =>
            {
                fixed (byte* ptr = &Intrinsics.GetData(obj))
                {
                    _manualResetEvent.WaitOne();
                }
            });
        }

        /// <summary>
        /// Gets the memory address of the pinned object.
        /// </summary>
        [NotNull]
        public void* Address
        {
            get
            {
                return Intrinsics.GetPointer(_target);
            }
        }

        /// <summary>
        /// Allocates a PinningHandle for the specified object.
        /// </summary>
        /// <param name="obj">The object to be pinned.</param>
        /// <returns>A PinningHandle instance.</returns>
        [NotNull]
        public static PinningHandle Alloc([NotNull] object obj)
        {
            ArgumentNullException.ThrowIfNull(obj, nameof(obj));
            return new PinningHandle(obj);
        }

        /// <summary>
        /// Frees the resources associated with the PinningHandle.
        /// </summary>
        /// <param name="pinningHandle">The PinningHandle to be freed.</param>
        public static void Free([NotNull] PinningHandle pinningHandle)
        {
            pinningHandle.Free();
        }

        /// <summary>
        /// Frees the resources associated with the PinningHandle.
        /// </summary>
        /// <returns>True if the resources were freed successfully; otherwise, false.</returns>
        public bool Free()
        {
            if (_manualResetEvent == null)
                return false;

            _manualResetEvent.Set();
            _target = null!;
            return true;
        }
    }
}
