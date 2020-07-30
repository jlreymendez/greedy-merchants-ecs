using System.Collections;
using UnityEngine.AddressableAssets;

namespace GreedyMerchants.ECS.Unity.Extensions
{
    public static class AssetReferenceExtensions
    {
        public static T GetAsset<T>(this AssetReference assetRef) where T : class
        {
            return assetRef.Asset as T;
        }

        public static IEnumerator LoadAssetTask<T>(this AssetReference assetRef)
        {
            var loader = assetRef.LoadAssetAsync<T>();
            while (loader.IsDone == false) { yield return null; }
        }
    }
}