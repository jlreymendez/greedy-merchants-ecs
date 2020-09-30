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
        public float Time;

        public ShipCollisionData(EGID id, int layer, float time)
        {
            EntityId = id;
            Layer = layer;
            Time = time;
        }
    }
}