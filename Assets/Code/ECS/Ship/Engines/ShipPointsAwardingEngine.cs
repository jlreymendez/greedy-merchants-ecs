using System.Collections;
using GreedyMerchants.ECS.Extensions.Svelto;
using GreedyMerchants.Unity;
using Svelto.ECS;

namespace GreedyMerchants.ECS.Ship
{
    public class ShipPointsAwardingEngine : IQueryingEntitiesEngine, ITickingEngine
    {
        readonly int _pointsPerCoin;
        readonly int _pointsPerKill;

        public ShipPointsAwardingEngine(int pointsPerCoin, int pointsPerKill)
        {
            _pointsPerCoin = pointsPerCoin;
            _pointsPerKill = pointsPerKill;
        }

        public EntitiesDB entitiesDB { get; set; }
        public void Ready() { }

        public GameTickScheduler tickScheduler => GameTickScheduler.Update;
        public int Order => (int) GameEngineOrder.Logic;

        public IEnumerator Tick()
        {
            while (true)
            {
                Process();
                yield return null;
            }
        }

        void Process()
        {
            var shipQuery = entitiesDB.QueryEntities<PointsComponent, ShipViewComponent>(ShipGroups.AllShipGroupsSnapshot);
            foreach (var ((points, shipViews, count), group) in shipQuery.groups)
            {
                for (var i = 0; i < count; i++)
                {
                    points[i].Points = points[i].Coins * _pointsPerCoin + points[i].ShipsSunk * _pointsPerKill;
                }
            }
        }
    }
}