using System;
using GreedyMerchants.ECS.Player;
using GreedyMerchants.ECS.Ship;
using Svelto.ECS;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace GreedyMerchants.Tests.Runtime.Utils
{
    public class PlayerBuilder
    {
        public Tuple<EGID, GameObject> Build()
        {
            var (ship, implementors) = The.GameObjectFactory.BuildForEntity(_prefab);
            var initializer = The.EntityFactory.BuildEntity<PlayerShipDescriptor>(_id, _group, implementors);

            initializer.Init(new ShipLevelComponent { Level = _level, NextLevel = _level });
            initializer.Init(new PointsComponent { Coins = _coins });

            ship.transform.position = _position;
            ship.transform.rotation = _rotation;

            return new Tuple<EGID, GameObject>(initializer.EGID, ship);
        }

        public PlayerBuilder WithId(uint id)
        {
            _id = id;
            return this;
        }

        public PlayerBuilder WithGroup(ExclusiveGroupStruct group)
        {
            _group = group;
            return this;
        }

        public PlayerBuilder WithPrefab(GameObject prefab)
        {
            _prefab = prefab;
            return this;
        }

        public PlayerBuilder WithPosition(float3 position)
        {
            _position = position;
            return this;
        }

        public PlayerBuilder WithRotation(quaternion rotation)
        {
            _rotation = rotation;
            return this;
        }

        public PlayerBuilder WithLevel(ShipLevel level)
        {
            _level = level;
            return this;
        }

        public PlayerBuilder WithCoins(int coins)
        {
            _coins = coins;
            return this;
        }

        uint _id = 0;
        ExclusiveGroupStruct _group = PlayerGroups.PlayerShip;
        GameObject _prefab = PrefabUtility.LoadPrefabContents("Assets/Prefabs/Ship.prefab");

        float3 _position = float3.zero;
        quaternion _rotation = quaternion.identity;

        ShipLevel _level = ShipLevel.Normal;
        int _coins;

        public static implicit operator Tuple<EGID, GameObject>(PlayerBuilder builder)
        {
            return builder.Build();
        }

        public void Deconstruct(out EGID egid, out GameObject ship)
        {
            (egid, ship) = Build();
        }
    }
}