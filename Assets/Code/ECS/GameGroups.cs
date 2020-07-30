using Svelto.DataStructures;
using Svelto.ECS;

namespace GreedyMerchants.ECS
{
    public static class GameGroups
    {
        public static readonly ExclusiveGroup PlayerShip = new ExclusiveGroup();
        public static readonly ExclusiveGroup AiShip = new ExclusiveGroup();

        public static readonly FasterReadOnlyList<ExclusiveGroupStruct> Ships = new FasterList<ExclusiveGroupStruct>
            (new ExclusiveGroupStruct[] { PlayerShip, AiShip });
    }
}