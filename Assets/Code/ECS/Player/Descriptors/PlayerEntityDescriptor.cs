using GreedyMerchants.ECS.Ship;
using Svelto.ECS;
using Svelto.ECS.Extensions.Unity;

namespace GreedyMerchants.ECS.Player
{
    public class PlayerEntityDescriptor : IEntityDescriptor
    {
        public IComponentBuilder[] componentsToBuild
        {
            get => new IComponentBuilder[]
            {
                new ComponentBuilder<PointsComponent>(),
                new ComponentBuilder<ShipComponent>(),
                new ComponentBuilder<ShipViewComponent>(),
                new ComponentBuilder<EGIDTrackerViewComponent>(),
            };
        }
    }
}