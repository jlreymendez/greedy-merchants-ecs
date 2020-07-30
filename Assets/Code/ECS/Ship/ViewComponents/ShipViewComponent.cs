using GreedyMerchants.ECS.Common;
using Svelto.ECS;
using Svelto.ECS.Hybrid;

namespace GreedyMerchants.ECS.Ship
{
    public struct ShipViewComponent : IEntityViewComponent
    {
        public ITransformComponent Transform;
        public ISpriteRendererComponent Renderer;

        public EGID ID { get; set; }
    }
}