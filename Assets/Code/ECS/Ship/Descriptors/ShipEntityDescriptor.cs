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
                new ComponentBuilder<ShipViewComponent>(),
                new ComponentBuilder<EGIDTrackerViewComponent>(),
            };
        }
    }
}