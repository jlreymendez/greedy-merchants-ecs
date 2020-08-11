using Svelto.ECS;
using Unity.Mathematics;

namespace GreedyMerchants.ECS.Ship
{
    public struct ShipNavigationComponent : IEntityComponent
    {
        public uint2 GridCell;
        public uint2 TargetGridCell;
        public bool4 Neighbors;
    }
}