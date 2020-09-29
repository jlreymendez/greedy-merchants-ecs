using Svelto.ECS;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace GreedyMerchants.ECS.HUD
{
    public class HUDSpawnEngine : IQueryingEntitiesEngine
    {
        AssetReference _sceneReference;

        public HUDSpawnEngine(AssetReference sceneReference)
        {
            _sceneReference = sceneReference;
        }

        public EntitiesDB entitiesDB { get; set; }

        public async void Ready()
        {
            if (string.IsNullOrEmpty(_sceneReference.AssetGUID)) return;
            var result = _sceneReference.LoadSceneAsync(LoadSceneMode.Additive);
            await result.Task;
        }
    }
}