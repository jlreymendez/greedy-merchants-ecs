using System.Collections;
using GreedyMerchants.ECS.Coin;
using Svelto.ECS;
using UnityEngine;

namespace GreedyMerchants.ECS.Ship
{
    public class ShipCoinPickupEngine : IQueryingEntitiesEngine
    {
        Consumer<ShipComponent> _consumer;

        public ShipCoinPickupEngine(IEntityStreamConsumerFactory consumerFactory)
        {
            _consumer = consumerFactory.GenerateConsumer<ShipComponent>("CoinPickup", 20);
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
                    if (ship.Collision.Layer != GameLayers.CoinLayer) continue;
                    ProcessCollision(ship);
                }

                yield return null;
            }
        }


        void ProcessCollision(ShipComponent ship)
        {
            ref var coin = ref entitiesDB.QueryEntity<CoinComponent>(ship.Collision.EntityId);

            if (coin.Picked == false)
            {
                coin.Picked = true;
                ref var points = ref entitiesDB.QueryEntity<PointsComponent>(ship.ID);
                points.Coins++;
            }
        }
    }
}