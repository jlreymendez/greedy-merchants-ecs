#if UNITY_COLLECTIONS
using System;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Unity.Jobs.LowLevel.Unsafe;
using Allocator = Svelto.Common.Allocator;

namespace Svelto.ECS.DataStructures.Unity
{
    public unsafe readonly struct AtomicNativeBags:IDisposable
    {
        [global::Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
        readonly NativeBag* _data;
        readonly Allocator _allocator;
        readonly uint _threadsCount;

        public uint count => _threadsCount;

        public AtomicNativeBags(Allocator allocator)
        {
            _allocator    = allocator;
            _threadsCount = JobsUtility.MaxJobThreadCount + 1;

            var bufferSize = MemoryUtilities.SizeOf<NativeBag>();
            var bufferCount = _threadsCount;
            var allocationSize = bufferSize * bufferCount;

            var ptr = (byte*)MemoryUtilities.Alloc((uint) allocationSize, allocator);
            MemoryUtilities.MemClear((IntPtr) ptr, (uint) allocationSize);

            for (int i = 0; i < bufferCount; i++)
            {
                var bufferPtr = (NativeBag*)(ptr + bufferSize * i);
                var buffer = new NativeBag(allocator);
                MemoryUtilities.CopyStructureToPtr(ref buffer, (IntPtr) bufferPtr);
            }

            _data = (NativeBag*)ptr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref NativeBag GetBuffer(int index)
        {
            return ref MemoryUtilities.ArrayElementAsRef<NativeBag>((IntPtr) _data, index);
        }

        public void Dispose()
        {
            for (int i = 0; i < _threadsCount; i++)
            {
                GetBuffer(i).Dispose();
            }
            MemoryUtilities.Free((IntPtr) _data, _allocator);
        }

        public void Clear()
        {
            for (int i = 0; i < _threadsCount; i++)
            {
                GetBuffer(i).Clear();
            }
        }
    }
}
#endif