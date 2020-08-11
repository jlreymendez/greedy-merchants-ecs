using Svelto.ECS;

namespace GreedyMerchants.ECS.Grid
{
    public class GridEntityDescriptor : IEntityDescriptor
    {
        public IComponentBuilder[] componentsToBuild
        {
            get => new IComponentBuilder[] { new ComponentBuilder<GridComponent>() };
        }
    }
}