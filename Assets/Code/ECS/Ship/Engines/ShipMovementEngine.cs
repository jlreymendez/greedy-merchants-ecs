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
            var entities = entitiesDB.QueryEntities<ShipComponent, ShipViewComponent>(GameGroups.Ships).entities;
            var enumerator = entities.GetEnumerator();

            while (enumerator.MoveNext())
            {
                var components = enumerator.Current;
                ref var ship = ref components.entityComponentA;
                ref var shipViews = ref components.entityComponentB;

                shipViews.Transform.Position += ship.Speed * ship.Direction * + _time.DeltaTime;
                shipViews.Transform.Right = ship.Direction;
            }
        }
    }
}