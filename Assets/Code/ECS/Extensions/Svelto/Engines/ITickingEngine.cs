using System.Collections;
using GreedyMerchants.Unity;
using Svelto.ECS;

namespace GreedyMerchants.ECS.Extensions.Svelto
{
    public interface ITickingEngine : IEngine
    {
        GameTickScheduler tickScheduler { get; }
        int Order { get; }
        IEnumerator Tick();
    }
}