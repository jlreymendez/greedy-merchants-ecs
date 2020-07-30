using Svelto.ECS;
using Unity.Mathematics;

namespace GreedyMerchants.ECS.Grid
{
    public struct GridCellComponent : IEntityComponent
    {
        public uint2 Position;
        public float2 WorldCenter;
    }
}