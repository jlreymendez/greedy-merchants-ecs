using Svelto.DataStructures;
using Svelto.ECS;

namespace GreedyMerchants.ECS.Ship
{
    public static class ShipGroups
    {
        public static FasterReadOnlyList<ExclusiveGroupStruct> SunkShipGroups = GroupTag<SUNK>.Groups;
        public static FasterReadOnlyList<ExclusiveGroupStruct> AliveShipGroups = GroupTag<AFLOAT>.Groups;

        // Note: this needs to be a property, it can't be cached since it depends on multiple tags.
            // However simply exposing this without any API difference is a bad idea,
            // since engines might decide to cache the result thinking it is safe to do so.
            // So I have renamed this to Snapshot as a warning sign against caching.
        public static FasterReadOnlyList<ExclusiveGroupStruct> AllShipGroupsSnapshot => new FasterList<ExclusiveGroupStruct>()
            .AddRange(AliveShipGroups)
            .AddRange(SunkShipGroups);
    }

    public class AFLOAT : GroupTag<AFLOAT> {}
    public class SUNK : GroupTag<SUNK> {}
}