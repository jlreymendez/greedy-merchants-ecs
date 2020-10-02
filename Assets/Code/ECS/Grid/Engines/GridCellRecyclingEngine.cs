using System.Collections;
using GreedyMerchants.ECS.Coin;
using GreedyMerchants.ECS.Extensions.Svelto;
using GreedyMerchants.Unity;
using Svelto.ECS;

namespace GreedyMerchants.ECS.Grid.Engines
{
    // review: Implement using new Svelto.ECS filters?
    public class GridCellRecyclingEngine : IQueryingEntitiesEngine, ITickingEngine, IReactOnSwap<CoinViewComponent>
    {
        IEntityFunctions _functions;
        GridUtils _gridUtils;

        public GridCellRecyclingEngine(IEntityFunctions functions, GridUtils gridUtils)
        {
            _functions = functions;
            _gridUtils = gridUtils;
        }

        public EntitiesDB entitiesDB { get; set; }
        public void Ready() { }

        public GameTickScheduler tickScheduler => GameTickScheduler.Update;
        public int Order => (int) GameEngineOrder.Output;

        public IEnumerator Tick()
        {
            while (true)
            {

                yield return null;
            }
        }

        public void MovedTo(ref CoinViewComponent view, ExclusiveGroupStruct previousGroup, EGID egid)
        {
            // Free the corresponding grid cell when a coin gets recycled.
            if (egid.groupID == CoinGroups.RecycledCoinsGroup)
            {
                var cellId = new EGID(_gridUtils.WorldToEnitityId(view.Transform.Position.xy), GridGroups.GridWaterHasCoinGroup);
                if (entitiesDB.Exists<GridCellComponent>(cellId) == false) return;

                _functions.SwapEntityGroup<GridCellEntityDescriptor>(cellId, GridGroups.GridWaterNoCoinGroup);
            }
            else if (egid.groupID == CoinGroups.SpawnedCoinsGroup)
            {
                var cellId = new EGID(_gridUtils.WorldToEnitityId(view.Transform.Position.xy), GridGroups.GridWaterNoCoinGroup);
                if (entitiesDB.Exists<GridCellComponent>(cellId) == false) return;

                _functions.SwapEntityGroup<GridCellEntityDescriptor>(cellId, GridGroups.GridWaterHasCoinGroup);
            }
        }
    }
}