using Svelto.ECS;
using Svelto.ECS.Hybrid;

namespace GreedyMerchants.ECS.Match
{
    public struct MatchViewComponent : IEntityViewComponent
    {
        public ITimerHudComponent TimerHud;

        public EGID ID { get; set; }
    }
}