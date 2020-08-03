using Svelto.ECS;
using Unity.Mathematics;

namespace GreedyMerchants.ECS.Grid
{
    public struct GridCellComponent : IEntityComponent, INeedEGID
    {
        public uint2 Position;
        public float2 WorldCenter;

        public EGID ID { get; set; }
    }
}