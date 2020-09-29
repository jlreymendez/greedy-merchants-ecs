using Svelto.ECS;

namespace GreedyMerchants.ECS.Ship
{
    public struct PointsComponent : IEntityComponent
    {
        public int Points;
        public int Coins;
        public int ShipsSunk;
    }
}