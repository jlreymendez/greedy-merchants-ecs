using Svelto.ECS.Extensions.Unity;
using UnityEngine;

namespace GreedyMerchants.Tests.Runtime.Utils
{
    public static class TearDownUtils
    {
        public static void DestroyAllEntityGameObjects()
        {
            var gameObjects = GameObject.FindObjectsOfType<EGIDHolderImplementor>();
            for (var i = 0; i < gameObjects.Length; i++)
            {
                GameObject.Destroy(gameObjects[i].gameObject);
            }
        }
    }
}