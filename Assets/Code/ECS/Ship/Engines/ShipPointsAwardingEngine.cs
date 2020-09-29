using System.Collections;
using Svelto.ECS;

namespace GreedyMerchants.ECS.Ship
{
    public class ShipPointsAwardingEngine : IQueryingEntitiesEngine
    {
        readonly int _pointsPerCoin;
        readonly int _pointsPerKill;

        public ShipPointsAwardingEngine(int pointsPerCoin, int pointsPerKill)
        {
            _pointsPerCoin = pointsPerCoin;
            _pointsPerKill = pointsPerKill;
        }

        public EntitiesDB entitiesDB { get; set; }
        public void Ready()
        {
            Tick().Run();
        }

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
            var shipQuery = entitiesDB.QueryEntities<PointsComponent, ShipViewComponent>(ShipGroups.Ships);
            foreach (var (points, shipViews, count) in shipQuery.groups)
            {
                for (var i = 0; i < count; i++)
                {
                    points[i].Points = points[i].Coins * _pointsPerCoin + points[i].ShipsSunk * _pointsPerKill;
                }
            }
        }
    }
}