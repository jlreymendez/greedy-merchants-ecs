using Svelto.ECS;
using Unity.Mathematics;

namespace GreedyMerchants.ECS.AI
{
    public struct AiTarget : IEntityComponent, INeedEGID
    {
        public EntityLocator Locator;
        public uint2 Position;

        public EGID ID { get; set; }
    }
}