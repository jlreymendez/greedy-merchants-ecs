using System;
using System.Collections;
using GreedyMerchants.Data.Ship;
using GreedyMerchants.ECS.Common;
using GreedyMerchants.ECS.Player;
using GreedyMerchants.ECS.Unity;
using GreedyMerchants.ECS.Unity.Extensions;
using Svelto.ECS;
using Svelto.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace GreedyMerchants.ECS.Ship
{
    public class ShipSpawningEngine : IQueryingEntitiesEngine
    {
        IEntityFactory _entityFactory;
        GameObjectFactory _gameObjectFactory;
        Transform[] _spawnPoints;
        ShipDefinition _shipDefinition;

        public ShipSpawningEngine(IEntityFactory entityFactory, GameObjectFactory gameObjectFactory, Transform[] spawnPoints, ShipDefinition shipDefinition)
        {
            _entityFactory = entityFactory;
            _gameObjectFactory = gameObjectFactory;
            _spawnPoints = spawnPoints;
            _shipDefinition = shipDefinition;
        }

        public EntitiesDB entitiesDB { get; set; }

        public void Ready()
        {
            Tick().Run();
        }

        IEnumerator Tick()
        {
            yield return InitialSpawning();
        }

        IEnumerator InitialSpawning()
        {
            var loadTasks = new ParallelTaskCollection();
            loadTasks.Add(_shipDefinition.Prefab.LoadAssetTask<GameObject>());
            loadTasks.Add(_shipDefinition.MerchantSprite.LoadAssetTask<Sprite>());
            loadTasks.Add(_shipDefinition.PirateSprite.LoadAssetTask<Sprite>());
            foreach (var sprite in _shipDefinition.ShipSprites)
            {
                loadTasks.Add(sprite.LoadAssetTask<Sprite>());
            }

            yield return loadTasks;

            uint id = 0;
            for (var i = 0; i < _spawnPoints.Length; i++)
            {
                var spawn = _spawnPoints[i];
                var (ship, implementors) =
                    _gameObjectFactory.BuildForEntity(_shipDefinition.Prefab.GetAsset<GameObject>(), spawn.position, spawn.rotation);

                var shipInitializer = _entityFactory.BuildEntity<PlayerEntityDescriptor>(id++, GameGroups.PlayerShip, implementors);
                shipInitializer.Init(new ShipComponent {
                    Speed = _shipDefinition.Speed,
                    Direction = math.mul(spawn.rotation, new float3(1, 0, 0))
                });

                var spriteRenderer = ship.GetComponent<SpriteRendererImplementor>();
                spriteRenderer.Sprites = new Sprite[]
                {
                    _shipDefinition.ShipSprites[i].GetAsset<Sprite>(),
                    _shipDefinition.MerchantSprite.GetAsset<Sprite>(),
                    _shipDefinition.PirateSprite.GetAsset<Sprite>()
                };
                spriteRenderer.Sprite = (int)ShipType.Normal;
            }
        }
    }
}