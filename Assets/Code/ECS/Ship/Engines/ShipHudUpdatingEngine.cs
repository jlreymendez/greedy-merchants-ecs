using System.Collections;
using Svelto.ECS;
using Svelto.ECS.Experimental;

namespace GreedyMerchants.ECS.Ship
{
    public class ShipHudUpdatingEngine : IQueryingEntitiesEngine
    {
        public EntitiesDB entitiesDB { get; set; }
        public void Ready()
        {
            Tick().Run();
        }

        public IEnumerator Tick()
        {
            // Wait for ships to get spawned to reset values.
            while (true)
            {
                var groups = new QueryGroups(ShipGroups.Ships).WithAny<ShipComponent>(entitiesDB);
                if (groups.result.count > 0) break;
                yield return null;
            }

            InitialSetup();

            while (true)
            {
                Process();
                yield return null;
            }
        }

        void InitialSetup()
        {
            var shipQuery = entitiesDB.QueryEntities<ShipComponent, ShipViewComponent>(ShipGroups.Ships);
            foreach (var (ship, shipViews, count) in shipQuery.groups)
            {
                for (var i = 0; i < count; i++)
                {
                    shipViews[i].CoinsHud.Coins = 0;
                    shipViews[i].PointsHud.Color = ship[i].Color;
                    shipViews[i].PointsHud.Points = 0;
                }
            }
        }

        void Process()
        {
            var shipQuery = entitiesDB.QueryEntities<PointsComponent, ShipViewComponent>(ShipGroups.Ships);
            foreach (var (points, shipViews, count) in shipQuery.groups)
            {
                for (var i = 0; i < count; i++)
                {
                    shipViews[i].CoinsHud.Coins = points[i].Coins;
                    shipViews[i].PointsHud.Points = points[i].Points;
                }
            }
        }
    }
}