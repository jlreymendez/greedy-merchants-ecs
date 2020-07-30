using System.Collections;
using GreedyMerchants.ECS.Unity;
using Svelto.ECS;
using Unity.Mathematics;

namespace GreedyMerchants.ECS.Ship
{
    public class ShipMovementEngine : IQueryingEntitiesEngine
    {
        ITime _time;

        public ShipMovementEngine(ITime time)
        {
            _time = time;
        }

        public EntitiesDB entitiesDB { get; set; }

        public void Ready()
        {
            Tick().Run();
        }

        IEnumerator Tick()
        {
            while (true)
            {
                Process();

                yield return null;
            }
        }

        void Process()
        {
            var query = entitiesDB.QueryEntities<ShipComponent, ShipViewComponent>(GameGroups.Ships);
            foreach (var (ships, shipViews, count) in query.groups)
            {
                for (var i = 0; i < count; i++)
                {
                    shipViews[i].Transform.Position += ships[i].Speed * ships[i].Direction * +_time.DeltaTime;
                    shipViews[i].Transform.Right = ships[i].Direction;
                }
            }
        }
    }
}