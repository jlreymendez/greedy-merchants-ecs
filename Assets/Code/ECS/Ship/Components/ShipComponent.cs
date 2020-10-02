using GreedyMerchants.Data.Ship;
using Svelto.ECS;
using Unity.Mathematics;

namespace GreedyMerchants.ECS.Ship
{
    public struct ShipComponent : IEntityComponent, INeedEGID
    {
        public ShipColor Color;
        public float Speed;
        public int2 Direction;
        public bool IsSinking;

        public ShipCollisionData Collision;
        public EGID ID { get; set; }
    }
}