using System.Diagnostics.CodeAnalysis;
using Svelto.DataStructures;
using Svelto.ECS;

namespace GreedyMerchants.ECS.Grid
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class GridGroups
    {
        public class GRID_CELL : GroupTag<GRID_CELL> {}
        public class WATER : GroupTag<WATER> {}
        public class LAND : GroupTag<LAND> {}
        public class HAS_COIN : GroupTag<HAS_COIN> {}
        public class NO_COIN : GroupTag<NO_COIN> {}

        public class GridLandGroupCompound : GroupCompound<GRID_CELL, LAND> {}
        public class GridWaterGroupCompound : GroupCompound<GRID_CELL, WATER> {}
        public class GridWaterHasCoinGroupCompound : GroupCompound<GRID_CELL, WATER, NO_COIN> {}
        public class GridWaterNoCoinGroupCompound : GroupCompound<GRID_CELL, WATER, HAS_COIN> {}

        public static readonly ExclusiveGroupStruct Grid = new ExclusiveGroup();

        public static readonly ExclusiveGroupStruct GridLandGroup = GridLandGroupCompound.BuildGroup;
        public static readonly ExclusiveGroupStruct GridWaterHasCoinGroup = GridWaterHasCoinGroupCompound.BuildGroup;
        public static readonly ExclusiveGroupStruct GridWaterNoCoinGroup = GridWaterNoCoinGroupCompound.BuildGroup;

        public static readonly FasterReadOnlyList<ExclusiveGroupStruct> GridWaterGroups = GridWaterGroupCompound.Groups;
    }
}

