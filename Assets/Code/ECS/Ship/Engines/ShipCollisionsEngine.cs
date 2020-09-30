using System;
using Svelto.ECS;

namespace GreedyMerchants.ECS.Ship
{
    public class ShipCollisionsEngine : IQueryingEntitiesEngine, IReactOnAddAndRemove<ShipViewComponent>
    {
        readonly Action<EGID, ShipCollisionData> _onCollision;

        public ShipCollisionsEngine()
        {
            _onCollision = OnCollision;
        }

        public EntitiesDB entitiesDB { get; set; }

        public void Ready() {}

        void OnCollision(EGID sender, ShipCollisionData collisionData)
        {
            ref var ship = ref entitiesDB.QueryEntity<ShipComponent>(sender);
            if (ship.Collision.Time < collisionData.Time)
            {
                ship.Collision = collisionData;
            }
            entitiesDB.PublishEntityChange<ShipComponent>(sender);
        }

        public void Add(ref ShipViewComponent shipView, EGID egid)
        {
            var dispatcher = new DispatchOnSet<ShipCollisionData>(egid);
            dispatcher.NotifyOnValueSet(_onCollision);
            shipView.Trigger.HitChange = dispatcher;
        }

        public void Remove(ref ShipViewComponent shipView, EGID egid)
        {
            shipView.Trigger.HitChange = null;
        }
    }
}