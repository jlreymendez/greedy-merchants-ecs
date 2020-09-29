using Svelto.ECS;

namespace GreedyMerchants.ECS.Match
{
    public class MatchDescriptor : IEntityDescriptor
    {
        public IComponentBuilder[] componentsToBuild => new IComponentBuilder[]
        {
            new ComponentBuilder<TimerComponent>(),
            new ComponentBuilder<MatchViewComponent>()
        };
    }
}