using System;
using System.Collections;
using GreedyMerchants.ECS.Extensions.Svelto;
using GreedyMerchants.ECS.Unity;
using GreedyMerchants.Unity;
using Svelto.ECS;

namespace GreedyMerchants.ECS.Ship
{
    public class ShipCollisionsEngine : IQueryingEntitiesEngine, IReactOnAddAndRemove<ShipViewComponent>, ITickingEngine
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
            ref var ship = ref entitiesDB.QueryEntity<ShipComponent>(sender);
            ship.Collision = collisionData;
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

        public GameTickScheduler tickScheduler => GameTickScheduler.Update;
        public int Order => (int) GameEngineOrder.Physics;

        public IEnumerator Tick()
        {
            while (true)
            {
                TickStep();
                yield return null;
            }

            void TickStep()
            {
                var query = entitiesDB.QueryEntities<ShipComponent>(ShipGroups.AliveShips);
                foreach (var (ships, count) in query.groups)
                {
                    for (var i = 0; i < count; i++)
                    {
                        ref var ship = ref ships[i];
                        if (_lastStepTime < ship.Collision.Time)
                        {
                            entitiesDB.PublishEntityChange<ShipComponent>(ship.ID);
                        }
                    }
                }

                _lastStepTime = _time.CurrentTime;
            }
        }
    }
}