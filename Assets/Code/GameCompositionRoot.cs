using GreedyMerchants.ECS.AI;
using GreedyMerchants.ECS.Coin;
using GreedyMerchants.ECS.Grid;
using GreedyMerchants.ECS.Grid.Engines;
using GreedyMerchants.ECS.Match.Engines;
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
        IEntityStreamConsumerFactory _entityConsumerFactory;
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
            _entityConsumerFactory = _enginesRoot.GenerateConsumerFactory();

            _time = new Time();
            _gameObjectFactory = new GameObjectFactory();
            _gridUtils = new GridUtils(context.GridDefinition);
            _seed = context.Seed == 0 ? (uint)Random.Range(int.MinValue, int.MinValue) : context.Seed;

            AddMatchEngines(context);
            AddGridEngines(context);
            AddShipEngines(context);
            AddPlayerEngines(context);
            AddCoinEngines(context);
            AddAiEngines(context);
        }

        void AddMatchEngines(GameContext context)
        {
            _enginesRoot.AddEngine(new MatchTimeCountingEngine(_entityFactory, _gameObjectFactory, context.TimerHUDCanvas, _time, context.MatchTime));
        }

        void AddGridEngines(GameContext context)
        {
            var gridLand = new GridTilemapRepresentation(context.Grid, context.LandTilemap, _gridUtils);
            _enginesRoot.AddEngine(new GridSpawningEngine(_entityFactory, _gridUtils, gridLand));
        }

        void AddShipEngines(GameContext context)
        {
            _enginesRoot.AddEngine(new ShipSpawningEngine(_seed, _entityFactory, _entityFunctions, _gameObjectFactory, context.ShipSpawns, context.Ship));
            _enginesRoot.AddEngine(new ShipMovementEngine(_time, _gridUtils));
            _enginesRoot.AddEngine(new ShipCollisionsEngine());
            _enginesRoot.AddEngine(new ShipCoinPickupEngine(_entityConsumerFactory));
            _enginesRoot.AddEngine(new ShipAttackEngine(_entityConsumerFactory, _entityFunctions, context.Ship));
            _enginesRoot.AddEngine(new ShipSinkingEngine());
            _enginesRoot.AddEngine(new ShipLevelConversionEngine(context.Ship));
            _enginesRoot.AddEngine(new ShipPointsAwardingEngine(context.PointsPerCoin, context.PointsPerKill));
            _enginesRoot.AddEngine(new ShipHudUpdatingEngine(context.PointsHUDCanvas));
        }

        void AddPlayerEngines(GameContext context)
        {
            _enginesRoot.AddEngine(new PlayerInputEngine(new PlayerInput()));
        }

        void AddCoinEngines(GameContext context)
        {
            _enginesRoot.AddEngine(new CoinSpawningEngine(_seed, context.CoinDefinition, _entityFactory, _gameObjectFactory, _entityFunctions, _time));
            _enginesRoot.AddEngine(new CoinAnimationEngine(_time));
            _enginesRoot.AddEngine(new CoinRecyclerEngine(_entityFunctions, _gridUtils));
        }

        void AddAiEngines(GameContext context)
        {
            _enginesRoot.AddEngine(new AIShipTargetSelectionEngine(_gridUtils));
            _enginesRoot.AddEngine(new AIShipSteeringEngine(_entityConsumerFactory, _gridUtils));
        }

        public void OnContextDestroyed()
        {
            _enginesRoot.Dispose();
            TaskRunner.StopAndCleanupAllDefaultSchedulers();
        }

        public void OnContextCreated<T>(T contextHolder) { }
    }
}