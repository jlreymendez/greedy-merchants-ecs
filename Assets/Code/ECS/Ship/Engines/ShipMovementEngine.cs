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
            while (true)
            {
                Process();

                yield return null;
            }
        }

        void Process()
        {
            var query = entitiesDB.QueryEntities<ShipComponent, ShipViewComponent>(ShipGroups.Ships);
            foreach (var (ships, shipViews, count) in query.groups)
            {
                for (var i = 0; i < count; i++)
                {
                    ref var ship = ref ships[i];
                    ref var shipView = ref shipViews[i];

                    // Update direction.
                    var right = math.round(shipView.Transform.Right);
                    var currentPosition = shipView.Transform.Position;
                    var isTurning = math.abs(math.dot(right, ship.Direction)) < MinDistance;
                    var cellCenter = new float3(_gridUtils.CellToCenterPosition(ship.GridCell), 0);
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
                    ship.TargetGridCell = ship.GridCell + (uint2)math.sign(right).xy;
                    if (isTurning)
                    {
                        var directionToCenter = math.sign(cellCenter - currentPosition);
                        if (right.Equals(directionToCenter))
                        {
                            ship.TargetGridCell = ship.GridCell;
                        }
                    }

                    // Check grid for collisions.
                    var targetCellId = _gridUtils.CellToEntityId(ship.TargetGridCell);
                    // note: this is a personal Svelto.ECS extension in GreedyMerchants.ECS.Extensions.Svelto to check for
                        // an entity id inside multiple groups with a single API call.
                        // There is an open discussion in the Svelto.ECS development team whether it is a good idea to
                        // provide this API, since it is hiding the performance cost behind it.
                    var validTarget = entitiesDB.Exists<GridCellComponent>(targetCellId, GridGroups.GridWaterGroups);

                    if (validTarget == false)
                    {
                        ship.TargetGridCell = ship.GridCell;
                    }

                    // Advance position.
                    var targetPosition = new float3(_gridUtils.CellToCenterPosition(ship.TargetGridCell), 0);
                    var distanceToTarget = math.distance(targetPosition, currentPosition);
                    var advanceDistance = ship.Speed * _time.DeltaTime;

                    ship.GridCell = _gridUtils.WorldToCellPosition(currentPosition.xy);
                    shipView.Transform.Position = currentPosition + right * (distanceToTarget < advanceDistance ? distanceToTarget : advanceDistance);
                    shipView.Transform.Right = right;
                }
            }
        }
    }
}
