using Svelto.DataStructures;
using Svelto.ECS;

namespace GreedyMerchants.ECS.Ship
{
    public static class ShipGroups
    {
        public static readonly ExclusiveGroupStruct PlayerShip = GroupCompound<SHIP, PLAYER>.BuildGroup;

        public static readonly ExclusiveGroupStruct AiPirateShip = GroupCompound<SHIP, AI, MERCHANT>.BuildGroup;
        public static readonly ExclusiveGroupStruct AiMerchantShip = GroupCompound<SHIP, AI, PIRATE>.BuildGroup;
        public static readonly ExclusiveGroupStruct AiNormalShip = GroupCompound<SHIP, AI, NORMAL>.BuildGroup;

        public static readonly ExclusiveGroupStruct SunkShip = GroupTag<SUNK_SHIP>.BuildGroup;

        public static readonly FasterReadOnlyList<ExclusiveGroupStruct> AliveShips = GroupTag<SHIP>.Groups;
        public static readonly FasterReadOnlyList<ExclusiveGroupStruct> Ships = new FasterList<ExclusiveGroupStruct>()
            .AddRange(AliveShips)
            .Add(SunkShip);
    }

    public class SHIP : GroupTag<SHIP> {}
    public class SUNK_SHIP: GroupTag<SUNK_SHIP> {}

    public class AI : GroupTag<AI> {}
    public class PLAYER : GroupTag<PLAYER> {}

    public class PIRATE : GroupTag<PIRATE> {}
    public class MERCHANT : GroupTag<MERCHANT> {}
    public class NORMAL : GroupTag<NORMAL> {}
}