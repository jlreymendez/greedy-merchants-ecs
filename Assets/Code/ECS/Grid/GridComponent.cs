using Svelto.ECS;
using Svelto.ECS.DataStructures;

namespace GreedyMerchants.ECS.Grid
{
    public struct GridComponent : IEntityComponent
    {
        public NativeDynamicArray WalkableGrid;
    }
}