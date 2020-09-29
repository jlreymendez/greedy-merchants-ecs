using System;
using System.Collections;
using GreedyMerchants.Data.Ship;
using GreedyMerchants.ECS.AI;
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
            GroupCompound<AI_SHIP, AFLOAT>.BuildGroup.SetTagSwap<AFLOAT, SUNK>(GroupCompound<SUNK, AI_SHIP>.BuildGroup);
            GroupCompound<PLAYER_SHIP, AFLOAT>.BuildGroup.SetTagSwap<AFLOAT, SUNK>(GroupCompound<SUNK, PLAYER_SHIP>.BuildGroup);

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

            EntityComponentInitializer shipInitializer;
            if (control == ShipControl.Player)
            {
                shipInitializer = _entityFactory.BuildEntity<PlayerShipDescriptor>(id, PlayerGroups.PlayerShip, implementors);
            }
            else
            {
                shipInitializer = _entityFactory.BuildEntity<AiShipDescriptor>(id, AiGroups.AiShip, implementors);
            }

            shipInitializer.Init(new ShipComponent {
                Speed = _shipDefinition.Speed,
                Direction = new int2(math.round(spawn.transform.right).xy)
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
            if (GroupTagExtensions.Contains<SUNK>(egid.groupID))
            {
                Respawn(shipView, egid).Run();
            }
        }
        IEnumerator Respawn(ShipViewComponent shipView, EGID egid)
        {
            yield return _respawnWait;

            Relocate(shipView, egid);

            var targetGroup = egid.groupID.SwapTag<AFLOAT>();

            if (GroupTagExtensions.Contains<PLAYER_SHIP>(egid.groupID))
            {
                _functions.SwapEntityGroup<PlayerShipDescriptor>(egid, targetGroup);
            }
            else
            {
                _functions.SwapEntityGroup<AiShipDescriptor>(egid, targetGroup);
            }

            shipView.UI.Visibility = true;

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