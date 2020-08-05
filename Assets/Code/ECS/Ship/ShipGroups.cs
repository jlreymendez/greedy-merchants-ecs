using Svelto.DataStructures;
using Svelto.ECS;

namespace GreedyMerchants.ECS.Ship
{
    public static class ShipGroups
    {
        public static readonly ExclusiveGroupStruct PlayerShip = GroupCompound<SHIP, PLAYER>.BuildGroup;
        public static readonly ExclusiveGroupStruct AiShip = GroupCompound<SHIP, AI>.BuildGroup;
        public static readonly ExclusiveGroupStruct SunkShip = GroupCompound<SHIP, SUNK>.BuildGroup;

        public static readonly FasterReadOnlyList<ExclusiveGroupStruct> Ships = GroupTag<SHIP>.Groups;

        public class SHIP : GroupTag<SHIP> {}
        public class SUNK: GroupTag<SUNK> {}
        public class AI : GroupTag<AI> {}
        public class PLAYER : GroupTag<PLAYER> {}
    }
}