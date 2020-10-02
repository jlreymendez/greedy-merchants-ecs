using System.Collections;
using GreedyMerchants.Data.Ship;
using GreedyMerchants.ECS.Extensions.Svelto;
using GreedyMerchants.Unity;
using Svelto.ECS;
using Unity.Mathematics;

namespace GreedyMerchants.ECS.Ship
{
    public class ShipAttackEngine : IQueryingEntitiesEngine, ITickingEngine
    {
        readonly int _coinDrops;
        Consumer<ShipComponent> _consumer;

        public ShipAttackEngine(IEntityStreamConsumerFactory consumerFactory, ShipDefinition shipDefinition)
        {
            _consumer = consumerFactory.GenerateConsumer<ShipComponent>("ShipAttack", 20);
            _coinDrops = shipDefinition.CoinDrop;
        }

        public EntitiesDB entitiesDB { get; set; }
        public void Ready() { }

        public GameTickScheduler tickScheduler => GameTickScheduler.Update;
        public int Order => (int) GameEngineOrder.Logic;

        public IEnumerator Tick()
        {
            while (true)
            {
                while (_consumer.TryDequeue(out ShipComponent ship))
                {
                    if (ship.Collision.Layer != GameLayers.ShipLayer) continue;
                    ProcessCollision(ref ship);
                }

                yield return null;
            }
        }

        void ProcessCollision(ref ShipComponent ship)
        {
            // Note: we are handling multiple collisions on each frame, make sure state is still valid.
            if (entitiesDB.Exists<ShipComponent>(ship.Collision.EntityId) == false) return;
            if (entitiesDB.Exists<ShipComponent>(ship.ID) == false) return;

            var shipLevel = entitiesDB.QueryEntity<ShipLevelComponent>(ship.ID);
            ref var points = ref entitiesDB.QueryEntity<PointsComponent>(ship.ID);
            var otherShipLevel = entitiesDB.QueryEntity<ShipLevelComponent>(ship.Collision.EntityId);
            ref var otherPoints = ref entitiesDB.QueryEntity<PointsComponent>(ship.Collision.EntityId);
            ref var otherShip = ref entitiesDB.QueryEntity<ShipComponent>(ship.Collision.EntityId);

            if (shipLevel.Level > otherShipLevel.Level)
            {
                // Steal some coins from it.
                var coinDrop = math.min(otherPoints.Coins, _coinDrops);
                points.Coins += coinDrop;
                points.ShipsSunk++;
                otherPoints.Coins -= coinDrop;
                otherShip.IsSinking = true;
            }
        }
    }
}