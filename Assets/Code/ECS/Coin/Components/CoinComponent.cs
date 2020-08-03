using Svelto.ECS;

namespace GreedyMerchants.ECS.Coin
{
    public struct CoinComponent : IEntityComponent
    {
        public float TimeToRespawn;
        public bool Picked;
    }
}