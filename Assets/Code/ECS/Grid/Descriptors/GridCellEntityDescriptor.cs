using Svelto.ECS;

namespace GreedyMerchants.ECS.Grid
{
    public class GridCellEntityDescriptor : IEntityDescriptor
    {
        public IComponentBuilder[] componentsToBuild
        {
            get => new IComponentBuilder[]
            {
                new ComponentBuilder<GridCellComponent>(),
            };
        }
    }
}