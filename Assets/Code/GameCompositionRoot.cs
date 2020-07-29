using Svelto.Context;
using Svelto.ECS;
using Svelto.ECS.Schedulers.Unity;

namespace GreedyMerchants
{
    public class GameCompositionRoot : ICompositionRoot
    {
        UnityEntitiesSubmissionScheduler _scheduler;
        EnginesRoot _enginesRoot;

        public void OnContextInitialized<T>(T contextHolder)
        {
            _scheduler = new UnityEntitiesSubmissionScheduler();
            _enginesRoot = new EnginesRoot(_scheduler);
        }

        public void OnContextDestroyed()
        {
        }

        public void OnContextCreated<T>(T contextHolder) { }
    }
}