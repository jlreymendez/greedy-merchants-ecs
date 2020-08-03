using System;
using System.Collections.Generic;
using Svelto.ECS.Extensions.Unity;
using Svelto.ECS.Hybrid;
using Unity.Mathematics;
using UnityEngine;

namespace GreedyMerchants.ECS.Unity
{
    public class GameObjectFactory
    {
        public GameObject Build(GameObject prefab)
        {
            return GameObject.Instantiate(prefab);
        }

        public Tuple<GameObject, List<IImplementor>> BuildForEntity(GameObject prefab)
        {
            return BuildForEntity(prefab, Vector3.zero, Quaternion.identity);
        }

        public Tuple<GameObject, List<IImplementor>> BuildForEntity(GameObject prefab, Vector3 position)
        {
            return BuildForEntity(prefab, position, Quaternion.identity);
        }

        public Tuple<GameObject, List<IImplementor>> BuildForEntity(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            var go = GameObject.Instantiate(prefab, position, rotation);
            go.AddComponent<EGIDHolderImplementor>();
            var implementors = new List<IImplementor>(go.GetComponentsInChildren<IImplementor>(true));

            return new Tuple<GameObject, List<IImplementor>>(go, implementors);
        }
    }
}