using GreedyMerchants.ECS.Coin;
using Svelto.ECS;

namespace GreedyMerchants.ECS.Grid.Engines
{
    public class GridCellCoinFilteringEngine : IQueryingEntitiesEngine,
        IReactOnSwap<CoinViewComponent>,
        IReactOnAddAndRemove<GridCellComponent>
    {
        GridUtils _gridUtils;
        FilterGroup _freeCellFilter;

        public GridCellCoinFilteringEngine(GridUtils gridUtils)
        {
            _gridUtils = gridUtils;
        }

        public EntitiesDB entitiesDB { get; set; }

        public void Ready()
        {
            _freeCellFilter = entitiesDB.GetFilters()
                .CreateOrGetFilterForGroup<GridCellComponent>(GridGroups.FreeCellFilter, GridGroups.GridWaterGroup);
        }

        public void MovedTo(ref CoinViewComponent view, ExclusiveGroupStruct previousGroup, EGID egid)
        {
            var mapper = entitiesDB.QueryMappedEntities<GridCellComponent>(GridGroups.GridWaterGroup);

            // Free the corresponding grid cell when a coin gets recycled.
            if (egid.groupID == CoinGroups.RecycledCoinsGroup)
            {
                var cellId = new EGID(_gridUtils.WorldToEnitityId(view.Transform.Position.xy), GridGroups.GridWaterGroup);
                if (entitiesDB.Exists<GridCellComponent>(cellId) == false) return;

                _freeCellFilter.Add(cellId.entityID, mapper);
            }
            else if (egid.groupID == CoinGroups.SpawnedCoinsGroup)
            {
                var cellId = new EGID(_gridUtils.WorldToEnitityId(view.Transform.Position.xy), GridGroups.GridWaterGroup);
                if (entitiesDB.Exists<GridCellComponent>(cellId) == false) return;

                _freeCellFilter.Remove(cellId.entityID);
            }
        }

        public void Add(ref GridCellComponent entityComponent, EGID egid)
        {
            if (egid.groupID == GridGroups.GridWaterGroup)
            {
                var mapper = entitiesDB.QueryMappedEntities<GridCellComponent>(GridGroups.GridWaterGroup);
                _freeCellFilter.Add(egid.entityID, mapper);
            }
        }

        public void Remove(ref GridCellComponent entityComponent, EGID egid)
        {
            if (egid.groupID == GridGroups.GridWaterGroup)
            {
                var mapper = entitiesDB.QueryMappedEntities<GridCellComponent>(GridGroups.GridWaterGroup);
                _freeCellFilter.TryRemove(egid.entityID);
                _freeCellFilter.RebuildIndicesOnStructuralChange(mapper);
            }
        }
    }
}