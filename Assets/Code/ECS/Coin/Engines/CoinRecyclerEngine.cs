using System.Collections;
using Code.ECS.Coin.Descriptors;
using Svelto.ECS;
using Svelto.Tasks;
using Svelto.Tasks.Unity;
using UnityEngine;

namespace GreedyMerchants.ECS.Coin
{
    public class CoinRecyclerEngine : IQueryingEntitiesEngine
    {
        IEntityFunctions _functions;

        public CoinRecyclerEngine(IEntityFunctions functions)
        {
            _functions = functions;
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
                    }
                }

                yield return null;
            }
        }
    }
}