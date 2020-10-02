using System;
using Code.ECS.Coin.Descriptors;
using GreedyMerchants.ECS.Coin;
using GreedyMerchants.ECS.Player;
using GreedyMerchants.ECS.Ship;
using Svelto.ECS;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace GreedyMerchants.Tests.Runtime.Utils
{
    public class CoinBuilder
    {
        public Tuple<EGID, GameObject> Build()
        {
            var (ship, implementors) = The.GameObjectFactory.BuildForEntity(_prefab);
            var initializer = The.EntityFactory.BuildEntity<CoinEntityDescriptor>(_id, _group, implementors);

            ship.transform.position = _position;

            return new Tuple<EGID, GameObject>(initializer.EGID, ship);
        }

        public CoinBuilder WithId(uint id)
        {
            _id = id;
            return this;
        }

        public CoinBuilder WithGroup(ExclusiveGroupStruct group)
        {
            _group = group;
            return this;
        }

        public CoinBuilder WithPrefab(GameObject prefab)
        {
            _prefab = prefab;
            return this;
        }

        public CoinBuilder WithPosition(float3 position)
        {
            _position = position;
            return this;
        }

        uint _id = 0;
        ExclusiveGroupStruct _group = CoinGroups.SpawnedCoinsGroup;
        GameObject _prefab = PrefabUtility.LoadPrefabContents("Assets/Prefabs/Coin.prefab");

        float3 _position = float3.zero;

        public static implicit operator Tuple<EGID, GameObject>(CoinBuilder builder)
        {
            return builder.Build();
        }

        public void Deconstruct(out EGID egid, out GameObject coin)
        {
            (egid, coin) = Build();
        }
    }
}