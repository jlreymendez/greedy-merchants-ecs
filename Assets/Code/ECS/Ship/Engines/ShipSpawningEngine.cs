using System;
using System.Collections;
using GreedyMerchants.Data.Ship;
using GreedyMerchants.ECS.Common;
using GreedyMerchants.ECS.Extensions.Svelto;
using GreedyMerchants.ECS.Grid;
using GreedyMerchants.ECS.Player;
using GreedyMerchants.ECS.Unity;
using GreedyMerchants.ECS.Unity.Extensions;
using Svelto.ECS;
using Svelto.Tasks;
using Svelto.Tasks.Enumerators;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace GreedyMerchants.ECS.Ship
{
    public class ShipSpawningEngine : IQueryingEntitiesEngine, IReactOnSwap<ShipViewComponent>
    {
        readonly IEntityFactory _entityFactory;
        readonly IEntityFunctions _functions;
        readonly GameObjectFactory _gameObjectFactory;
        readonly Transform[] _spawnPoints;
        readonly ShipDefinition _shipDefinition;
        WaitForSecondsEnumerator _respawnWait;
        WaitForSecondsEnumerator _transitionWait;
        WaitForSecondsEnumerator _animationWait;
        Random _random;

        public ShipSpawningEngine(uint seed, IEntityFactory entityFactory, IEntityFunctions functions, GameObjectFactory gameObjectFactory, Transform[] spawnPoints, ShipDefinition shipDefinition)
        {
            _entityFactory = entityFactory;
            _functions = functions;
            _gameObjectFactory = gameObjectFactory;
            _spawnPoints = spawnPoints;
            _shipDefinition = shipDefinition;
            _respawnWait = new WaitForSecondsEnumerator(_shipDefinition.TimeToRespawn);
            _transitionWait = new WaitForSecondsEnumerator(_shipDefinition.RespawnTransitionTime);
            _animationWait = new WaitForSecondsEnumerator(_shipDefinition.BlinkAnimationTime);
            _random = new Random(seed);
        }

        public EntitiesDB entitiesDB { get; set; }

        public void Ready()
        {
            Tick().Run();
        }

        IEnumerator Tick()
        {
            // Register possible transitions.
            GroupCompound<SHIP, AI>.BuildGroup.SetTagSwap<SHIP, SUNK_SHIP>(GroupCompound<SUNK_SHIP, AI>.BuildGroup);
            GroupCompound<SHIP, PLAYER>.BuildGroup.SetTagSwap<SHIP, SUNK_SHIP>(GroupCompound<SUNK_SHIP, PLAYER>.BuildGroup);

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

            for (var i = 0; i < _spawnPoints.Length; i++)
            {
                Spawn((uint)i, _spawnPoints[i], i == 0 ? ShipControl.Player : ShipControl.Ai);
            }
        }

        void Spawn(uint id, Transform spawn, ShipControl control)
        {
            var (ship, implementors) =
                _gameObjectFactory.BuildForEntity(_shipDefinition.Prefab.GetAsset<GameObject>(), spawn.position, spawn.rotation);

            var group = control == ShipControl.Player ? ShipGroups.PlayerShip : ShipGroups.AiShip;
            var shipInitializer = _entityFactory.BuildEntity<ShipEntityDescriptor>(id, group, implementors);
            shipInitializer.Init(new ShipComponent {
                Speed = _shipDefinition.Speed,
                Direction = math.round(spawn.transform.right)
            });
            shipInitializer.Init(new ShipLevelComponent() { Level =  ShipLevel.Normal });

            var spriteRenderer = ship.GetComponent<SpriteRendererImplementor>();
            spriteRenderer.Sprites = new Sprite[]
            {
                _shipDefinition.MerchantSprite.GetAsset<Sprite>(),
                _shipDefinition.ShipSprites[id].GetAsset<Sprite>(),
                _shipDefinition.PirateSprite.GetAsset<Sprite>()
            };
            spriteRenderer.Sprite = (int)ShipLevel.Normal;
        }

        public void MovedTo(ref ShipViewComponent shipView, ExclusiveGroupStruct previousGroup, EGID egid)
        {
            if (GroupTagExtensions.Contains<SUNK_SHIP>(egid.groupID))
            {
                Respawn(shipView, egid).Run();
            }
        }
        IEnumerator Respawn(ShipViewComponent shipView, EGID egid)
        {
            yield return _respawnWait;

            Relocate(shipView, egid);

            var targetGroup = egid.groupID.SwapTag<SHIP>();
            _functions.SwapEntityGroup<ShipEntityDescriptor>(egid, targetGroup);

            var render = false;
            while (_transitionWait.MoveNext())
            {
                render = !render;
                shipView.Renderer.Render = render;

                yield return _animationWait;
            }

            shipView.Renderer.Render = true;
            shipView.Physics.Enable = true;
        }

        void Relocate(ShipViewComponent shipView, EGID egid)
        {
            var (cells, count) = entitiesDB.QueryEntities<GridCellComponent>(GridGroups.GridWaterHasCoinGroup);
            var index = _random.NextInt(0, count);

            ref var navigation = ref entitiesDB.QueryEntity<ShipNavigationComponent>(egid);
            navigation.GridCell = cells[index].Position;
            shipView.Transform.Position = new float3(cells[index].WorldCenter, 0);
        }
    }
}