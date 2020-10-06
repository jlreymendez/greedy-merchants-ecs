#if UNITY_BURST
using Svelto.ECS.DataStructures.Unity;

namespace Svelto.ECS
{
    public readonly struct NativeEntityRemove
    {
        readonly AtomicNativeBags _removeQueue;
        readonly int             _indexRemove;

        internal NativeEntityRemove(AtomicNativeBags EGIDsToRemove, int indexRemove)
        {
            _removeQueue = EGIDsToRemove;
            _indexRemove = indexRemove;
        }

        public void RemoveEntity(EGID egid, int threadIndex)
        {
            var simpleNativeBag = _removeQueue.GetBuffer(threadIndex);
            
            simpleNativeBag.Enqueue(_indexRemove);
            simpleNativeBag.Enqueue(egid);
        }
    }
}
#endif