using System.Collections;
using GreedyMerchants.Data.Ship;
using GreedyMerchants.ECS.Player;
using Svelto.ECS;
using Unity.Mathematics;

namespace GreedyMerchants.ECS.Ship
{
    public class ShipAttackEngine : IQueryingEntitiesEngine
    {
        readonly int _coinDrops;
        readonly IEntityFunctions _functions;
        Consumer<ShipComponent> _consumer;

        public ShipAttackEngine(IEntityStreamConsumerFactory consumerFactory, IEntityFunctions functions, ShipDefinition shipDefinition)
        {
            _consumer = consumerFactory.GenerateConsumer<ShipComponent>("ShipAttack", 20);
            _functions = functions;
        }

        public EntitiesDB entitiesDB { get; set; }

        public void Ready()
        {
            Tick().Run();
        }

        IEnumerator Tick()
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
            var shipLevel = entitiesDB.QueryEntity<ShipLevelComponent>(ship.ID);
            var otherShipLevel = entitiesDB.QueryEntity<ShipLevelComponent>(ship.Collision.EntityId);
            ref var points = ref entitiesDB.QueryEntity<PointsComponent>(ship.ID);
            ref var otherPoints = ref entitiesDB.QueryEntity<PointsComponent>(ship.ID);

            if (shipLevel.Level > otherShipLevel.Level)
            {
                // Steal some coins from it.
                var coinDrop = math.min(otherPoints.Coins, _coinDrops);
                points.Coins += coinDrop;
                otherPoints.Coins -= coinDrop;

                // Sunk the other ship.
                _functions.SwapEntityGroup<PlayerEntityDescriptor>(ship.Collision.EntityId, ShipGroups.SunkShip);
            }
        }
    }
}