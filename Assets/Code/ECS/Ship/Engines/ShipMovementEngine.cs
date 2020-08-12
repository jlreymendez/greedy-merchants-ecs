using System.Collections;
using GreedyMerchants.ECS.Grid;
using GreedyMerchants.ECS.Unity;
using GreedyMerchants.ECS.Extensions.Svelto;
using Svelto.ECS;
using Unity.Mathematics;

namespace GreedyMerchants.ECS.Ship
{
    public class ShipMovementEngine : IQueryingEntitiesEngine
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

        public void Ready()
        {
            Tick().Run();
        }

        IEnumerator Tick()
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
            var query = entitiesDB.QueryEntities<ShipComponent, ShipNavigationComponent, ShipViewComponent>(ShipGroups.AliveShips);
            var grid = entitiesDB.QueryEntity<GridComponent>(0, GridGroups.Grid);
            foreach (var (ships, navigations, views, count) in query.groups)
            {
                for (var i = 0; i < count; i++)
                {
                    ref var ship = ref ships[i];
                    ref var navigation = ref navigations[i];
                    ref var view = ref views[i];

                    // Update direction.
                    var right = math.round(view.Transform.Right);
                    var currentPosition = view.Transform.Position;
                    var isTurning = math.abs(math.dot(right, ship.Direction)) < MinDistance;
                    var cellCenter = new float3(_gridUtils.CellToCenterPosition(navigation.GridCell), 0);
                    if (isTurning)
                    {
                        // Ship is turning (only rotate if we are at the center of a tile.
                        if (math.distance(cellCenter, currentPosition) < MinDistance)
                        {
                            right = math.round(ship.Direction);
                            currentPosition = cellCenter;
                            isTurning = false;
                        }
                    }
                    else
                    {
                        // Ship is either keeping its direction or turning 180 degrees.
                        right = math.round(ship.Direction);
                    }

                    // Select target cell, if we are turning we might want to turn in the current cell.
                    navigation.TargetGridCell = navigation.GridCell + (uint2)math.sign(right).xy;
                    if (isTurning)
                    {
                        var directionToCenter = math.sign(cellCenter - currentPosition);
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
                    view.Transform.Position = currentPosition + right * (distanceToTarget < advanceDistance ? distanceToTarget : advanceDistance);
                    view.Transform.Right = right;

                    // Update cell if needed.
                    var currentCell = _gridUtils.WorldToCellPosition(currentPosition.xy);
                    navigation.GridCell = _gridUtils.WorldToCellPosition(currentPosition.xy);
                }
            }
        }
    }
}
