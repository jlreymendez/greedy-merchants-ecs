using System.Collections;
using Code.ECS.Coin.Descriptors;
using GreedyMerchants.ECS.Extensions.Svelto;
using GreedyMerchants.Unity;
using Svelto.ECS;

namespace GreedyMerchants.ECS.Coin
{
    public class CoinRecyclerEngine : IQueryingEntitiesEngine, ITickingEngine
    {
        IEntityFunctions _functions;

        public CoinRecyclerEngine(IEntityFunctions functions)
        {
            _functions = functions;
        }

        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        public GameTickScheduler tickScheduler => GameTickScheduler.Update;
        public int Order => (int) GameEngineOrder.Logic;

        public IEnumerator Tick()
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