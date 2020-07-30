using GreedyMerchants.ECS.Ship;
using Svelto.ECS;

namespace GreedyMerchants.ECS.Player
{
    public class PlayerEntityDescriptor : IEntityDescriptor
    {
        public IComponentBuilder[] componentsToBuild
        {
            get => new IComponentBuilder[]
            {
                new ComponentBuilder<ShipComponent>(),
                new ComponentBuilder<ShipViewComponent>(),
            };
        }
    }
}