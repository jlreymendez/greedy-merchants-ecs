using GreedyMerchants.ECS.AI;
using Svelto.ECS;
using Svelto.ECS.Extensions.Unity;

namespace GreedyMerchants.ECS.Ship
{
    public class ShipEntityDescriptor : IEntityDescriptor
    {
        public IComponentBuilder[] componentsToBuild
        {
            get => new IComponentBuilder[]
            {
                new ComponentBuilder<PointsComponent>(),
                new ComponentBuilder<ShipComponent>(),
                new ComponentBuilder<ShipLevelComponent>(),
                new ComponentBuilder<ShipNavigationComponent>(),
                new ComponentBuilder<ShipViewComponent>(),
                new ComponentBuilder<EGIDTrackerViewComponent>(),
                // note: Being lazy here. I should create different entity descriptors for ai and player.
                new ComponentBuilder<AiTarget>(),
                new ComponentBuilder<AiPath>(),
            };
        }
    }
}