using Svelto.ECS;

namespace GreedyMerchants.ECS.Ship
{
    public struct ShipLevelComponent : IEntityComponent, INeedEGID
    {
        public ShipLevel Level;
        public ShipLevel NextLevel;

        public EGID ID { get; set; }
    }
}