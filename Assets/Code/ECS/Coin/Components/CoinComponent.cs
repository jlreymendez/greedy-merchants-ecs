using Svelto.ECS;

namespace GreedyMerchants.ECS.Coin
{
    public struct CoinComponent : IEntityComponent, INeedEGID
    {
        public float TimeToRespawn;
        public bool Picked;
        public EGID ID { get; set; }
    }
}