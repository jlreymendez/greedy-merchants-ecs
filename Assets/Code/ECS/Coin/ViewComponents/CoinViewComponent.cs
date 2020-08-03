using GreedyMerchants.ECS.Common;
using Svelto.ECS;
using Svelto.ECS.Hybrid;

namespace GreedyMerchants.ECS.Coin
{
    public struct CoinViewComponent : IEntityViewComponent
    {
        public ISpriteRendererComponent Renderer;
        public ITransformComponent Transform;
        public IPhysicsComponent Physics;

        public EGID ID { get; set; }
    }
}