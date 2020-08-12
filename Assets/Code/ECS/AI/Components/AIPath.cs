using Svelto.ECS;
using Svelto.ECS.DataStructures;

namespace GreedyMerchants.ECS.AI
{
    public struct AiPath : IEntityComponent
    {
        public NativeDynamicArray Path;
        public uint Waypoint;
    }
}