using Svelto.DataStructures;
using Svelto.ECS;

namespace GreedyMerchants.ECS.Ship
{
    public static class ShipGroups
    {
        public static FasterReadOnlyList<ExclusiveGroupStruct> SunkShips => GroupTag<SUNK>.Groups;
        public static FasterReadOnlyList<ExclusiveGroupStruct> AliveShips => GroupTag<AFLOAT>.Groups;
        public static FasterReadOnlyList<ExclusiveGroupStruct> Ships => new FasterList<ExclusiveGroupStruct>()
            .AddRange(AliveShips)
            .AddRange(SunkShips);
    }

    public class AFLOAT : GroupTag<AFLOAT> {}
    public class SUNK : GroupTag<SUNK> {}
}