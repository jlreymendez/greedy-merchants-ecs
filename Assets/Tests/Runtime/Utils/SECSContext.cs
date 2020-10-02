using GreedyMerchants.ECS.Extensions.Svelto;
using GreedyMerchants.ECS.Unity;
using GreedyMerchants.Unity;
using Svelto.ECS;
using Svelto.ECS.Schedulers;
using Svelto.Tasks;

namespace GreedyMerchants.Tests.Runtime.Utils
{
    public class SECSContext
    {
        public readonly GameRunner Runner;
        public readonly EnginesRoot Root;
        public readonly EntitiesDB EntitiesDB;
        public readonly IEntityFactory EntityFactory;
        public readonly IEntityFunctions EntityFunctions;
        public readonly IEntityStreamConsumerFactory ConsumerFactory;
        public readonly IEntitiesSubmissionScheduler Scheduler;
        public readonly ITime Time;
        public readonly GameObjectFactory GameObjectFactory;
        readonly DummyEngine Engine;

        public SECSContext()
        {
            Runner = new GameRunner();
            Root = new EnginesRoot(Runner.SubmissionScheduler);
            Engine = new DummyEngine();
            Root.AddEngine(Engine);
            EntitiesDB = Engine.entitiesDB;
            EntityFactory = Root.GenerateEntityFactory();
            EntityFunctions = Root.GenerateEntityFunctions();
            ConsumerFactory = Root.GenerateConsumerFactory();
            Scheduler = Runner.SubmissionScheduler;
            GameObjectFactory = new GameObjectFactory();
            Time = Runner.Time;
        }

        public void AddEngine(IEngine engine)
        {
            Root.AddEngine(engine);
            if (engine is ITickingEngine) { Runner.AddTickEngine((ITickingEngine)engine); }
        }

        public void Play()
        {
            Runner.Play();
        }

        public void Dispose()
        {
            TaskRunner.StopAndCleanupAllDefaultSchedulers();
            Root.Dispose();
        }

        class DummyEngine : IQueryingEntitiesEngine
        {
            public EntitiesDB entitiesDB { get; set; }
            public void Ready() { }
        }
    }
}