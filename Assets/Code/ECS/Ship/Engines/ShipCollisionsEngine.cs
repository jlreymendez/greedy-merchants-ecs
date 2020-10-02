using System;
using GreedyMerchants.ECS.Unity;
using Svelto.ECS;

namespace GreedyMerchants.ECS.Ship
{
    public class ShipCollisionsEngine : IQueryingEntitiesEngine, IReactOnAddAndRemove<ShipViewComponent>
    {
        readonly Action<EGID, ShipCollisionData> _onCollision;
        ITime _time;
        float _lastStepTime;

        public ShipCollisionsEngine(ITime time)
        {
            _onCollision = OnCollision;
            _time = time;
            _lastStepTime = _time.CurrentTime;
        }

        public EntitiesDB entitiesDB { get; set; }

        public void Ready() {}

        void OnCollision(EGID sender, ShipCollisionData collisionData)
        {
            // Note: we are handling multiple collisions on each frame, make sure state is still valid.
            if (entitiesDB.Exists<ShipComponent>(sender) == false) return;
            if (entitiesDB.Exists<ShipComponent>(collisionData.EntityId) == false) return;

            ref var ship = ref entitiesDB.QueryEntity<ShipComponent>(sender);
            ship.Collision = collisionData;
            entitiesDB.PublishEntityChange<ShipComponent>(ship.ID);
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