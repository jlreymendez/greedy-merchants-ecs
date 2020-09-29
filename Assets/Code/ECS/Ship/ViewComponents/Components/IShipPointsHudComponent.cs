using GreedyMerchants.Data.Ship;

namespace GreedyMerchants.ECS.Ship
{
    public interface IShipPointsHudComponent
    {
        ShipColor Color { set; }
        int Points { set; }
    }
}