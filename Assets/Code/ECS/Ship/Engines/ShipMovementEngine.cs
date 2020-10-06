using System.Collections;
using GreedyMerchants.ECS.Extensions.Svelto;
using GreedyMerchants.ECS.Grid;
using GreedyMerchants.ECS.Unity;
using GreedyMerchants.Unity;
using Svelto.ECS;
using Unity.Mathematics;

namespace GreedyMerchants.ECS.Ship
{
    public class ShipMovementEngine : IQueryingEntitiesEngine, ITickingEngine
    {
        const float MinDistance = 0.02f;

        ITime _time;
        GridUtils _gridUtils;

        public ShipMovementEngine(ITime time, GridUtils gridUtils)
        {
            _time = time;
            _gridUtils = gridUtils;
        }

        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        public GameTickScheduler tickScheduler => GameTickScheduler.Update;
        public int Order => (int) GameEngineOrder.Movement;

        public IEnumerator Tick()
        {
            while (entitiesDB.Count<GridComponent>(GridGroups.Grid) == 0)
            {
                yield return null;
            }

            while (true)
            {
                Process();
                yield return null;
            }
        }

        void Process()
        {
            var query =
                entitiesDB.QueryEntities<ShipComponent, ShipNavigationComponent, ShipViewComponent>(ShipGroups.AliveShipGroups);
            var grid = entitiesDB.QueryEntity<GridComponent>(0, GridGroups.Grid);
            foreach (var (ships, navigations, views, count) in query.groups)
            {
                for (var i = 0; i < count; i++)
                {
                    ref var ship = ref ships[i];
                    ref var navigation = ref navigations[i];
                    ref var view = ref views[i];

                    // Update direction.
                    var right = new int2(math.round(view.Transform.Right).xy);
                    var currentPosition = view.Transform.Position;
                    var isTurning = math.abs(math.dot(right, ship.Direction)) < MinDistance;
                    var cellCenter = new float3(_gridUtils.CellToCenterPosition(navigation.GridCell), 0);
                    if (isTurning)
                    {
                        // Ship is turning (only rotate if we are at the center of a tile.
                        if (math.distance(cellCenter, currentPosition) < MinDistance)
                        {
                            right = ship.Direction;
                            currentPosition = cellCenter;
                            isTurning = false;
                        }
                    }
                    else
                    {
                        // Ship is either keeping its direction or turning 180 degrees.
                        right = ship.Direction;
                    }

                    // Select target cell, if we are turning we might want to turn in the current cell.
                    navigation.TargetGridCell = navigation.GridCell + (uint2)math.sign(right).xy;
                    if (isTurning)
                    {
                        var directionToCenter = new int2(math.sign(cellCenter - currentPosition).xy);
                        if (right.Equals(directionToCenter))
                        {
                            navigation.TargetGridCell = navigation.GridCell;
                        }
                    }

                    // Check grid for collisions.
                    var targetCellId = _gridUtils.CellToEntityId(navigation.TargetGridCell);
                    if (grid.WalkableGrid.Get<bool>(targetCellId) == false)
                    {
                        navigation.TargetGridCell = navigation.GridCell;
                    }

                    // Advance position.
                    var targetPosition = new float3(_gridUtils.CellToCenterPosition(navigation.TargetGridCell), 0);
                    var distanceToTarget = math.distance(targetPosition, currentPosition);
                    var advanceDistance = ship.Speed * _time.DeltaTime;

                    // Update view.
                    view.Transform.Right = new float3(right, 0);
                    view.Transform.Position = currentPosition + view.Transform.Right * (distanceToTarget < advanceDistance ? distanceToTarget : advanceDistance);

                    // Update cell if needed.
                    navigation.GridCell = _gridUtils.WorldToCellPosition(currentPosition.xy);
                }
            }
        }
    }
}
