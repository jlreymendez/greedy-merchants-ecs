using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace GreedyMerchants.Tests.Runtime.Utils
{
    public class AssetLoader<T> where T : Object
    {
        string _key;
        public T Result { get; private set; }

        public AssetLoader(string key)
        {
            _key = key;
        }

        public IEnumerator<T> Load()
        {
            var loader = Addressables.LoadAssetAsync<T>(_key);
            while (loader.IsDone == false) yield return null;
            Result = loader.Result;
        }

        public IEnumerator<T> Instantiate()
        {
            var loader = Addressables.LoadAssetAsync<T>(_key);
            while (loader.IsDone == false) yield return null;
            Result = Object.Instantiate(loader.Result);
        }

        public static implicit operator T(AssetLoader<T> loader)
        {
            return loader.Result;
        }
    }
}