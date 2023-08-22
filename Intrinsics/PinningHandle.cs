using JetBrains.Annotations;

namespace Cassowary.Intrinsics
{
    public unsafe class PinningHandle
    {
        private ManualResetEvent _manualResetEvent;
        private object _target;

        internal PinningHandle([NotNull] object obj)
        {
            _manualResetEvent = new ManualResetEvent(false);
            _target = obj;

            ThreadPool.QueueUserWorkItem(_ =>
            {
                fixed (byte* ptr = &Intrinsics.GetData(obj))
                {
                    _manualResetEvent.WaitOne();
                }
            });
        }

        public void* Address
        {
            get
            {
                return Intrinsics.GetPointer(_target);
            }
        }

        public static PinningHandle Alloc(object obj)
        {
            ArgumentNullException.ThrowIfNull(obj, nameof(obj));
            return new PinningHandle(obj);
        }

        public static void Free([NotNull] PinningHandle pinningHandle)
        {
            pinningHandle.Free();
        }

        public bool Free()
        {
            if (_manualResetEvent == null)
                return false;

            _manualResetEvent.Set();
            return true;
        }
    }
}
