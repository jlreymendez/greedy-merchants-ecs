using System.Collections;
using Code.ECS.Coin.Descriptors;
using GreedyMerchants.ECS.Grid;
using Svelto.ECS;
using Svelto.Tasks;
using Svelto.Tasks.Unity;
using UnityEngine;

namespace GreedyMerchants.ECS.Coin
{
    public class CoinRecyclerEngine : IQueryingEntitiesEngine
    {
        IEntityFunctions _functions;
        GridUtils _gridUtils;

        public CoinRecyclerEngine(IEntityFunctions functions, GridUtils gridUtils)
        {
            _functions = functions;
            _gridUtils = gridUtils;
        }

        public EntitiesDB entitiesDB { get; set; }

        public void Ready()
        {
            Tick().RunOnScheduler(StandardSchedulers.lateScheduler);
        }

        IEnumerator Tick()
        {
            while (true)
            {
                var (coins, coinViews, count) = entitiesDB.QueryEntities<CoinComponent, CoinViewComponent>(CoinGroups.SpawnedCoinsGroup);

                for (var i = 0; i < count; i++)
                {
                    if (coins[i].Picked)
                    {
                        coinViews[i].Physics.Enable = false;
                        _functions.SwapEntityGroup<CoinEntityDescriptor>(coins[i].ID, CoinGroups.RecycledCoinsGroup);

                        var cellEgid = new EGID(_gridUtils.WorldToEnitityId(coinViews[i].Transform.Position.xy), CoinGroups.CellsWithCoins);
                        _functions.SwapEntityGroup<GridCellEntityDescriptor>(cellEgid, CoinGroups.CellsWithoutCoins);
                    }
                }

                yield return null;
            }
        }
    }
}