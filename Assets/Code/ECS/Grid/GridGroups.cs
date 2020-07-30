using System.Diagnostics.CodeAnalysis;
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

        public class GridLandGroup : GroupCompound<GRID_CELL, LAND> {}
        public class GridWaterHasCoinGroup : GroupCompound<GRID_CELL, WATER, NO_COIN> {}
        public class GridWaterNoCoinGroup : GroupCompound<GRID_CELL, WATER, HAS_COIN> {}
    }
}

