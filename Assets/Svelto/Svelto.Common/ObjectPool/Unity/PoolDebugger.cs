#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Svelto.ObjectPool
{
    public class PoolDebugger : MonoBehaviour
    {
        public int numberOfObjectsCreatedSinceLastTime;
        public int lastNumberOfObjectsCreatedGreaterThanZero;
        public int numberOfObjectsReusedSinceLastTime;
        public int lastNumberOfObjectsReusedGreaterThanZero;
        public int numberOfObjectsRecycledSinceLastTime;
        public int lastNumberOfObjectsRecycledGreaterThanZero;

        public int secondsToWait;

        public List<ObjectPoolDebugStructureInt>    debugPoolInfo      = new List<ObjectPoolDebugStructureInt>();
        public List<ObjectPoolDebugStructureString> debugNamedPoolInfo = new List<ObjectPoolDebugStructureString>();

        public DateTime Later { get; private set; }

        internal void SetPool(IObjectPoolDebug objectPool)
        {
            _objectPool = objectPool;
        }

        public void FetchObjectCreated()
        {
            FetchObjectCreatedLite();

            SetLists();
        }

        void SetLists()
        {
            debugPoolInfo      = _objectPool.DebugPoolInfo(debugPoolInfo);
            debugNamedPoolInfo = _objectPool.DebugNamedPoolInfo(debugNamedPoolInfo);
        }

        void FetchObjectCreatedLite()
        {
            numberOfObjectsCreatedSinceLastTime = _objectPool.GetNumberOfObjectsCreatedSinceLastTime();

            if (numberOfObjectsCreatedSinceLastTime != 0)
                lastNumberOfObjectsCreatedGreaterThanZero = numberOfObjectsCreatedSinceLastTime;
        }

        public void FetchObjectReused()
        {
            FetchObjectReusedLite();

            SetLists();
        }

        void FetchObjectReusedLite()
        {
            numberOfObjectsReusedSinceLastTime = _objectPool.GetNumberOfObjectsReusedSinceLastTime();

            if (numberOfObjectsReusedSinceLastTime != 0)
                lastNumberOfObjectsReusedGreaterThanZero = numberOfObjectsReusedSinceLastTime;
        }

        public void FetchObjectRecycled()
        {
            FetchObjectRecycledLite();

            SetLists();
        }

        void FetchObjectRecycledLite()
        {
            numberOfObjectsRecycledSinceLastTime = _objectPool.GetNumberOfObjectsRecycledSinceLastTime();

            if (numberOfObjectsRecycledSinceLastTime != 0)
                lastNumberOfObjectsRecycledGreaterThanZero = numberOfObjectsRecycledSinceLastTime;
        }

        void Update()
        {
            if (secondsToWait > 0)
            {
                if (DateTime.Now >= Later)
                {
                    Later = DateTime.Now.AddSeconds(secondsToWait);

                    FetchObjectCreatedLite();
                    FetchObjectReusedLite();
                    FetchObjectRecycledLite();

                    SetLists();
                }
            }
        }

        IObjectPoolDebug _objectPool;
    }
}
#endif