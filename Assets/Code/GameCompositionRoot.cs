using GreedyMerchants.ECS.AI;
using GreedyMerchants.ECS.Coin;
using GreedyMerchants.ECS.Extensions.Svelto;
using GreedyMerchants.ECS.Grid;
using GreedyMerchants.ECS.Grid.Engines;
using GreedyMerchants.ECS.Match.Engines;
using GreedyMerchants.ECS.Player;
using GreedyMerchants.ECS.Ship;
using GreedyMerchants.ECS.Unity;
using GreedyMerchants.Unity;
using Svelto.Context;
using Svelto.ECS;
using Svelto.Tasks;
using UnityEngine;

namespace GreedyMerchants
{
    public class GameCompositionRoot : ICompositionRoot
    {
        GameRunner _runner;
        EnginesRoot _enginesRoot;
        IEntityFactory _entityFactory;
        IEntityFunctions _entityFunctions;
        IEntityStreamConsumerFactory _entityConsumerFactory;
        GameObjectFactory _gameObjectFactory;
        GridUtils _gridUtils;
        uint _seed;

        public void OnContextInitialized<T>(T contextHolder)
        {
            CompositionRoot(contextHolder as GameContext);
        }

        void CompositionRoot(GameContext context)
        {
            _runner = new GameRunner();
            _enginesRoot = new EnginesRoot(_runner.SubmissionScheduler);
            _entityFactory = _enginesRoot.GenerateEntityFactory();
            _entityFunctions = _enginesRoot.GenerateEntityFunctions();
            _entityConsumerFactory = _enginesRoot.GenerateConsumerFactory();

            _gameObjectFactory = new GameObjectFactory();
            _gridUtils = new GridUtils(context.GridDefinition);
            _seed = context.Seed == 0 ? (uint)Random.Range(int.MinValue, int.MinValue) : context.Seed;

            AddMatchEngines(context);
            AddGridEngines(context);
            AddShipEngines(context);
            AddPlayerEngines(context);
            AddCoinEngines(context);
            AddAiEngines(context);

            _runner.Play();
        }

        void AddMatchEngines(GameContext context)
        {
            AddEngine(new MatchTimeCountingEngine(_runner, _entityFactory, _gameObjectFactory, context.TimerHUDCanvas, context.MatchTime));
        }

        void AddGridEngines(GameContext context)
        {
            var gridLand = new GridTilemapRepresentation(context.Grid, context.LandTilemap, _gridUtils);
            AddEngine(new GridSpawningEngine(_entityFactory, _gridUtils, gridLand));
            AddEngine(new GridCellRecyclingEngine(_entityFunctions, _gridUtils));
        }

        void AddShipEngines(GameContext context)
        {
            AddEngine(new ShipSpawningEngine(_seed, _entityFactory, _entityFunctions, _gameObjectFactory, context.ShipSpawns, context.Ship));
            AddEngine(new ShipMovementEngine(_runner.Time, _gridUtils));
            AddEngine(new ShipCollisionsEngine(_runner.Time));
            AddEngine(new ShipCoinPickupEngine(_entityConsumerFactory));
            AddEngine(new ShipAttackEngine(_entityConsumerFactory, context.Ship));
            AddEngine(new ShipSinkingEngine(_entityFunctions));
            AddEngine(new ShipLevelConversionEngine(context.Ship));
            AddEngine(new ShipPointsAwardingEngine(context.PointsPerCoin, context.PointsPerKill));
            AddEngine(new ShipHudUpdatingEngine(context.PointsHUDCanvas));
        }

        void AddPlayerEngines(GameContext context)
        {
            AddEngine(new PlayerInputEngine(new PlayerInput()));
        }

        void AddCoinEngines(GameContext context)
        {
            AddEngine(new CoinSpawningEngine(_seed, context.CoinDefinition, _entityFactory, _gameObjectFactory, _entityFunctions, _runner.Time));
            AddEngine(new CoinAnimationEngine(_runner.Time));
            AddEngine(new CoinRecyclerEngine(_entityFunctions));
        }

        void AddAiEngines(GameContext context)
        {
            AddEngine(new AIShipTargetSelectionEngine(_gridUtils));
            AddEngine(new AIShipSteeringEngine(_entityConsumerFactory, _gridUtils));
        }

        void AddEngine(IEngine engine)
        {
            _enginesRoot.AddEngine(engine);
            if (engine is ITickingEngine)
            {
                _runner.AddTickEngine(engine as ITickingEngine);
            }
        }

        public void OnContextDestroyed()
        {
            _enginesRoot.Dispose();
            TaskRunner.StopAndCleanupAllDefaultSchedulers();
        }

        public void OnContextCreated<T>(T contextHolder) { }
    }
}