using System.Collections;
using GreedyMerchants.Data.Audio;
using GreedyMerchants.ECS.Coin;
using GreedyMerchants.ECS.Extensions.Svelto;
using GreedyMerchants.ECS.Player;
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

            // Coin may have been picked by another ship on the same frame.
            if (coin.Picked) return;

            coin.Picked = true;
            ref var points = ref entitiesDB.QueryEntity<PointsComponent>(ship.ID);
            points.Coins++;

            ref var shipView = ref entitiesDB.QueryEntity<ShipViewComponent>(ship.ID);
            shipView.CoinsHud.Coins = points.Coins;

            // For players only play coin pickup clip
            if (GroupTagExtensions.Contains<PLAYER_SHIP>(ship.ID.groupID))
            {
                shipView.Audio.PlayOneShot = ShipAudioType.CoinPick;
            }
        }
    }
}