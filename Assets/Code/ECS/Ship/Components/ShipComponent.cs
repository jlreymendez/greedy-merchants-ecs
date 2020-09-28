using Svelto.ECS;
using Unity.Mathematics;

namespace GreedyMerchants.ECS.Ship
{
    public struct ShipComponent : IEntityComponent, INeedEGID
    {
        public float Speed;
        public int2 Direction;

        public ShipCollisionData Collision;
        public EGID ID { get; set; }
    }
}