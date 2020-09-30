using System.Collections;
using GreedyMerchants.Data.Ship;
using GreedyMerchants.ECS.AI;
using GreedyMerchants.ECS.Extensions.Svelto;
using GreedyMerchants.ECS.Player;
using GreedyMerchants.Unity;
using Svelto.ECS;
using Unity.Mathematics;

namespace GreedyMerchants.ECS.Ship
{
    public class ShipAttackEngine : IQueryingEntitiesEngine, ITickingEngine
    {
        readonly int _coinDrops;
        readonly IEntityFunctions _functions;
        Consumer<ShipComponent> _consumer;

        public ShipAttackEngine(IEntityStreamConsumerFactory consumerFactory, IEntityFunctions functions, ShipDefinition shipDefinition)
        {
            _consumer = consumerFactory.GenerateConsumer<ShipComponent>("ShipAttack", 20);
            _coinDrops = shipDefinition.CoinDrop;
            _functions = functions;
        }

        public EntitiesDB entitiesDB { get; set; }
        public void Ready() { }

        public GameTickScheduler tickScheduler => GameTickScheduler.Update;
        public int Order => (int) GameEngineOrder.Physics;

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
            // Make sure collision still exists, might have been sunk by another ship.
            if (entitiesDB.Exists<ShipComponent>(ship.Collision.EntityId) == false) return;
            if (entitiesDB.Exists<ShipComponent>(ship.ID) == false) return;

            var shipLevel = entitiesDB.QueryEntity<ShipLevelComponent>(ship.ID);
            ref var points = ref entitiesDB.QueryEntity<PointsComponent>(ship.ID);
            var otherShipLevel = entitiesDB.QueryEntity<ShipLevelComponent>(ship.Collision.EntityId);
            ref var otherPoints = ref entitiesDB.QueryEntity<PointsComponent>(ship.Collision.EntityId);

            if (shipLevel.Level > otherShipLevel.Level)
            {
                // Steal some coins from it.
                var coinDrop = math.min(otherPoints.Coins, _coinDrops);
                points.Coins += coinDrop;
                points.ShipsSunk++;
                otherPoints.Coins -= coinDrop;

                // Sunk the other ship.
                // note: This semi abstracted engine shouldn't know which descriptor it is changing.
                    // This probably needs to be handled else where.
                var targetGroup = ship.Collision.EntityId.groupID.SwapTag<SUNK>();
                if (GroupTagExtensions.Contains<PLAYER_SHIP>(ship.Collision.EntityId.groupID))
                {
                    _functions.SwapEntityGroup<PlayerShipDescriptor>(ship.Collision.EntityId, targetGroup);
                }
                else
                {
                    _functions.SwapEntityGroup<AiShipDescriptor>(ship.Collision.EntityId, targetGroup);
                }
            }
        }
    }
}