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
        public class GridLandGroupCompound : GroupCompound<GRID_CELL, LAND> {}
        public class GridWaterGroupCompound : GroupCompound<GRID_CELL, WATER> {}

        public static readonly ExclusiveGroupStruct Grid = new ExclusiveGroup();

        public static readonly ExclusiveGroupStruct GridLandGroup = GridLandGroupCompound.BuildGroup;
        public static readonly ExclusiveGroupStruct GridWaterGroup = GridWaterGroupCompound.BuildGroup;

        public static int FreeCellFilter = 1;
    }
}

