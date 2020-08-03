using GreedyMerchants.ECS.Grid;
using Svelto.ECS;

namespace GreedyMerchants.ECS.Coin
{
    public static class CoinGroups
    {
        public static readonly ExclusiveGroup SpawnedCoinsGroup = new ExclusiveGroup();
        public static readonly ExclusiveGroup RecycledCoinsGroup = new ExclusiveGroup();
        public static readonly ExclusiveGroupStruct CellsWithoutCoins = GridGroups.GridWaterNoCoinGroup;
        public static readonly ExclusiveGroupStruct CellsWithCoins = GridGroups.GridWaterHasCoinGroup;
    }
}