using UnityEngine;

namespace GreedyMerchants.ECS
{
    public static class GameLayers
    {
        public static readonly int CoinLayer = LayerMask.NameToLayer("Coin");
        public static readonly int ShipLayer = LayerMask.NameToLayer("Ship");
    }
}