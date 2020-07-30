using GreedyMerchants.ECS.Ship;
using GreedyMerchants.ECS.Unity;
using Svelto.Context;
using Svelto.ECS;
using Svelto.ECS.Schedulers.Unity;
using Svelto.Tasks;
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

            AddShipEngines(context);
        }

        void AddShipEngines(GameContext context)
        {
            _enginesRoot.AddEngine(new ShipSpawningEngine(_entityFactory, _gameObjectFactory, context.ShipSpawns, context.Ship));
            _enginesRoot.AddEngine(new ShipMovementEngine(_time));
        }

        public void OnContextDestroyed()
        {
            _enginesRoot.Dispose();
            TaskRunner.StopAndCleanupAllDefaultSchedulers();
        }

        public void OnContextCreated<T>(T contextHolder) { }
    }
}