using Svelto.ECS;
using Svelto.ECS.Hybrid;

namespace GreedyMerchants.ECS.Common
{
    public struct TransformViewComponent : IEntityViewComponent
    {
        public ITransformComponent Transform;

        public EGID ID { get; set; }
    }
}