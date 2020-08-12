using GreedyMerchants.ECS.Ship;
using Svelto.ECS;
using Svelto.ECS.Extensions.Unity;

namespace GreedyMerchants.ECS.AI
{
    public class AiShipDescriptor : IEntityDescriptor
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
                new ComponentBuilder<AiTarget>(),
                new ComponentBuilder<AiPath>(),
            };
        }
    }
}