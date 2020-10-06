using System;
using System.Collections;
using Code.Data.Coin;
using Code.ECS.Coin.Descriptors;
using GreedyMerchants.ECS.Extensions.Svelto;
using GreedyMerchants.ECS.Grid;
using GreedyMerchants.ECS.Unity;
using GreedyMerchants.ECS.Unity.Extensions;
using GreedyMerchants.Unity;
using Svelto.ECS;
using Svelto.Tasks.Enumerators;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace GreedyMerchants.ECS.Coin
{
    public class CoinSpawningEngine : IQueryingEntitiesEngine,
        ITickingEngine,
        IReactOnAddAndRemove<CoinViewComponent>,
        IReactOnSwap<CoinViewComponent>, IReactOnSwap<CoinComponent>
    {
        readonly CoinDefinition _coinDefinition;
        readonly IEntityFunctions _functions;
        readonly IEntityFactory _entityFactory;
        readonly GameObjectFactory _gameObjectFactory;
        readonly ITime _time;
        Random _random;
        WaitForSubmissionEnumerator _submissionWait;
        FilterGroup _freeCellsFilter;

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
            _freeCellsFilter = entitiesDB.GetFilters()
                .CreateOrGetFilterForGroup<GridCellComponent>(GridGroups.FreeCellFilter, GridGroups.GridWaterGroup);
        }

        public void Add(ref CoinViewComponent coinView, EGID egid)
        {
            if (egid.groupID == CoinGroups.RecycledCoinsGroup)
            {
                coinView.Renderer.Render = false;
                coinView.Physics.Enable = false;
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
                EnableCoin(coinView).Run();
            }
        }

        public void MovedTo(ref CoinComponent coin, ExclusiveGroupStruct previousGroup, EGID egid)
        {
            if (egid.groupID == CoinGroups.SpawnedCoinsGroup)
            {
                coin.Picked = false;
                coin.TimeToRespawn = _coinDefinition.CoinsSpawnTime;
            }
        }

        public GameTickScheduler tickScheduler => GameTickScheduler.Early;
        public int Order => (int) GameEngineOrder.Init;

        public IEnumerator Tick()
        {
            _submissionWait = new WaitForSubmissionEnumerator(_functions, _entityFactory, entitiesDB);
            yield return ReadyAllCoins();

            while (true)
            {
                yield return _submissionWait;

                var (coins, coinViews, coinsCount) =
                    entitiesDB.QueryEntities<CoinComponent, CoinViewComponent>(CoinGroups.RecycledCoinsGroup);
                if (coinsCount == 0) continue;

                // Advance spawn timer.
                var coinToSpawn = -1;
                for (var i = 0; i < coinsCount; i++)
                {
                    coins[i].TimeToRespawn -= _time.DeltaTime;
                    if (coins[i].TimeToRespawn > 0) continue;

                    coinToSpawn = i;
                    break;
                }

                // Make sure there are available cells to drop a coin.
                var (cells, cellCount) = entitiesDB.QueryEntities<GridCellComponent>(GridGroups.GridWaterGroup);
                if (cellCount == 0 || _freeCellsFilter.filteredIndices.Count() == 0 || coinToSpawn < 0) continue;

                // Spawn one coin if found (recycling it).
                var availableCells = _freeCellsFilter.filteredIndices;
                var selectedCell = _random.NextUInt(0, (uint)availableCells.Count());
                var cell = cells[availableCells.Get(selectedCell)];

                var coinView = coinViews[coinToSpawn];
                _functions.SwapEntityGroup<CoinEntityDescriptor>(coinView.ID, CoinGroups.SpawnedCoinsGroup);

                coinView.Transform.Position = new float3(cell.WorldCenter, 0);
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
                _entityFactory.BuildEntity<CoinEntityDescriptor>(i, CoinGroups.RecycledCoinsGroup, implementors);
            }
        }

        IEnumerator EnableCoin(CoinViewComponent coinView)
        {
            coinView.Renderer.Render = true;
            coinView.Renderer.Sprite = 0;
            yield return new WaitForSecondsEnumerator(0.5f);
            coinView.Physics.Enable = true;
        }
    }
}