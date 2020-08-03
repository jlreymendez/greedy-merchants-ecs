using Svelto.ECS;
using Unity.Mathematics;

namespace GreedyMerchants.ECS.Ship
{
    public struct ShipComponent : IEntityComponent, INeedEGID
    {
        public float Speed;
        public float3 Direction;

        public uint2 GridCell;
        public uint2 TargetGridCell;

        public ShipCollisionData Collision;
        public EGID ID { get; set; }
    }
}