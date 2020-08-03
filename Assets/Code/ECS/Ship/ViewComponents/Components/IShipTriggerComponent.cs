using System;
using Svelto.ECS;

namespace GreedyMerchants.ECS.Ship
{
    public interface IShipTriggerComponent
    {
        DispatchOnSet<ShipCollisionData> HitChange { get; set; }
    }

    public struct ShipCollisionData
    {
        public EGID EntityId;
        public int Layer;

        public ShipCollisionData(EGID Id, int layer)
        {
            EntityId = Id;
            Layer = layer;
        }
    }
}