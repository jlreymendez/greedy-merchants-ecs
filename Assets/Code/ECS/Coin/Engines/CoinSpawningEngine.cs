using System;
using System.Collections;
using Code.Data.Coin;
using Code.ECS.Coin.Descriptors;
using GreedyMerchants.ECS.Grid;
using GreedyMerchants.ECS.Unity;
using GreedyMerchants.ECS.Unity.Extensions;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.Tasks.Enumerators;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace GreedyMerchants.ECS.Coin
{
    public class CoinSpawningEngine : IQueryingEntitiesEngine, IReactOnAddAndRemove<CoinViewComponent>, IReactOnSwap<CoinViewComponent>
    {
        readonly CoinDefinition _coinDefinition;
        readonly IEntityFunctions _functions;
        readonly IEntityFactory _entityFactory;
        readonly GameObjectFactory _gameObjectFactory;
        readonly ITime _time;
        Random _random;
        WaitForSecondsEnumerator _spawnWait;
        WaitForSubmissionEnumerator _submissionWait;

        public CoinSpawningEngine(uint seed, CoinDefinition coinDefinition, IEntityFactory entityFactory, GameObjectFactory gameObjectFactory, IEntityFunctions functions, ITime time)
        {
            _entityFactory = entityFactory;
            _gameObjectFactory = gameObjectFactory;
            _functions = functions;
            _coinDefinition = coinDefinition;
            _time = time;
            _random = new Random(seed);
        }

        public EntitiesDB entitiesDB { get; set; }

        public void Ready()
        {
            Tick().Run();
        }

        public void Add(ref CoinViewComponent coinView, EGID egid)
        {
            if (egid.groupID == CoinGroups.RecycledCoinsGroup)
            {
                coinView.Renderer.Render = false;
            }
        }

        public void Remove(ref CoinViewComponent entityComponent, EGID egid) { }

        public void MovedTo(ref CoinViewComponent coinView, ExclusiveGroupStruct previousGroup, EGID egid)
        {
            if (egid.groupID == CoinGroups.RecycledCoinsGroup)
            {
                coinView.Renderer.Render = false;
            }
            else
            {
                coinView.Renderer.Render = true;
                coinView.Renderer.Sprite = 0;
            }
        }

        IEnumerator Tick()
        {
            _spawnWait = new WaitForSecondsEnumerator(_coinDefinition.CoinsSpawnTime);
            _submissionWait = new WaitForSubmissionEnumerator(_functions, _entityFactory, entitiesDB);
            yield return ReadyAllCoins();

            while (true)
            {
                // yield return SpawnCoin();
                yield return _submissionWait;
                var (cells, cellCount) = entitiesDB.QueryEntities<GridCellComponent>(GridGroups.GridWaterNoCoinGroup);
                if (cellCount == 0) continue;

                var (coins, coinViews, coinsCount) =
                    entitiesDB.QueryEntities<CoinComponent, CoinViewComponent>(CoinGroups.RecycledCoinsGroup);
                if (coinsCount == 0) continue;

                // Advance spawn timer.
                for (var i = 0; i < coinsCount; i++)
                {
                    coins[i].TimeToRespawn -= _time.DeltaTime;
                }

                // Spawn first coin if ready.
                if (coins[0].TimeToRespawn <= 0)
                {
                    var cell = cells[_random.NextInt(0, cellCount)];
                    _functions.SwapEntityGroup<GridCellEntityDescriptor>(cell.ID, GridGroups.GridWaterHasCoinGroup);

                    // Recycle coin.
                    var coinView = coinViews[0];
                    _functions.SwapEntityGroup<CoinEntityDescriptor>(coinView.ID, CoinGroups.SpawnedCoinsGroup);

                    coinView.Transform.Position = new float3(cell.WorldCenter, 0);
                }
            }
        }

        IEnumerator ReadyAllCoins()
        {
            var loader = _coinDefinition.Prefab.LoadAssetTask<GameObject>();
            yield return loader;
            var prefab = _coinDefinition.Prefab.Asset as GameObject;

            for (uint i = 0; i < _coinDefinition.MaxCoins; i++)
            {
                var (coin, implementors) = _gameObjectFactory.BuildForEntity(prefab);
                var initializer = _entityFactory.BuildEntity<CoinEntityDescriptor>(i, CoinGroups.RecycledCoinsGroup, implementors);
            }
        }
    }
}