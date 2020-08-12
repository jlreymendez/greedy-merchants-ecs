using Svelto.DataStructures;
using Svelto.ECS;

namespace GreedyMerchants.ECS.Ship
{
    public static class ShipGroups
    {
        public static readonly FasterReadOnlyList<ExclusiveGroupStruct> SunkShips = GroupTag<SUNK>.Groups;
        public static readonly FasterReadOnlyList<ExclusiveGroupStruct> AliveShips = GroupTag<AFLOAT>.Groups;
        public static readonly FasterReadOnlyList<ExclusiveGroupStruct> Ships = new FasterList<ExclusiveGroupStruct>()
            .AddRange(AliveShips)
            .AddRange(SunkShips);
    }

    public class AFLOAT : GroupTag<AFLOAT> {}
    public class SUNK : GroupTag<SUNK> {}
}