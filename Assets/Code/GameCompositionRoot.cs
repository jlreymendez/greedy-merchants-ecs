using GreedyMerchants.ECS.Coin;
using GreedyMerchants.ECS.Grid;
using GreedyMerchants.ECS.Grid.Engines;
using GreedyMerchants.ECS.Player;
using GreedyMerchants.ECS.Ship;
using GreedyMerchants.ECS.Unity;
using Svelto.Context;
using Svelto.ECS;
using Svelto.ECS.Schedulers.Unity;
using Svelto.Tasks;
using UnityEngine;
using Time = GreedyMerchants.ECS.Unity.Time;

namespace GreedyMerchants
{
    public class GameCompositionRoot : ICompositionRoot
    {
        UnityEntitiesSubmissionScheduler _scheduler;
        EnginesRoot _enginesRoot;
        IEntityFactory _entityFactory;
        IEntityFunctions _entityFunctions;
        Time _time;
        GameObjectFactory _gameObjectFactory;
        GridUtils _gridUtils;
        uint _seed;

        public void OnContextInitialized<T>(T contextHolder)
        {
            CompositionRoot(contextHolder as GameContext);
        }

        void CompositionRoot(GameContext context)
        {
            _scheduler = new UnityEntitiesSubmissionScheduler();
            _enginesRoot = new EnginesRoot(_scheduler);
            _entityFactory = _enginesRoot.GenerateEntityFactory();
            _entityFunctions = _enginesRoot.GenerateEntityFunctions();

            _time = new Time();
            _gameObjectFactory = new GameObjectFactory();
            _gridUtils = new GridUtils(context.Grid, context.LandTilemap, context.GridDefinition);
            _seed = context.Seed == 0 ? (uint)Random.Range(int.MinValue, int.MinValue) : context.Seed;

            AddGridEngines(context);
            AddShipEngines(context);
            AddPlayerEngines(context);
            AddCoinEngines(context);
        }

        void AddGridEngines(GameContext context)
        {
            _enginesRoot.AddEngine(new GridSpawningEngine(_entityFactory, _gridUtils));
        }

        void AddShipEngines(GameContext context)
        {
            _enginesRoot.AddEngine(new ShipSpawningEngine(_entityFactory, _gameObjectFactory, context.ShipSpawns, context.Ship));
            _enginesRoot.AddEngine(new ShipMovementEngine(_time, _gridUtils));
        }

        void AddPlayerEngines(GameContext context)
        {
            _enginesRoot.AddEngine(new PlayerInputEngine(new PlayerInput()));
        }

        void AddCoinEngines(GameContext context)
        {
            _enginesRoot.AddEngine(new CoinSpawningEngine(_seed, context.CoinDefinition, _entityFactory, _gameObjectFactory, _entityFunctions, _time));
        }

        public void OnContextDestroyed()
        {
            _enginesRoot.Dispose();
            TaskRunner.StopAndCleanupAllDefaultSchedulers();
        }

        public void OnContextCreated<T>(T contextHolder) { }
    }
}