using System.Collections;
using System.Runtime.CompilerServices;
using GreedyMerchants.ECS.Extensions.Svelto;
using GreedyMerchants.ECS.Grid;
using GreedyMerchants.ECS.Ship;
using Svelto.Common;
using Svelto.ECS;
using Svelto.ECS.DataStructures;
using Unity.Mathematics;

namespace GreedyMerchants.ECS.AI
{
    public class AIShipSteeringEngine : IQueryingEntitiesEngine, IReactOnAddAndRemove<AiPath>, IReactOnSwap<AiPath>
    {
        GridUtils _gridUtils;
        Consumer<AiTarget> _consumer;
        IReactOnAddAndRemove<AiPath> _reactOnAddAndRemoveImplementation;

        public AIShipSteeringEngine(IEntityStreamConsumerFactory consumerFactory, GridUtils gridUtils)
        {
            _consumer = consumerFactory.GenerateConsumer<AiTarget>("AiShipSteering", 4);
            _gridUtils = gridUtils;
        }

        public EntitiesDB entitiesDB { get; set; }

        public void Add(ref AiPath aiPath, EGID egid)
        {
            aiPath.Path = NativeDynamicArray.Alloc<uint>(Allocator.Persistent, 10);
        }

        public void Remove(ref AiPath aiPath, EGID egid)
        {
            aiPath.Path.Dispose();
        }

        public void MovedTo(ref AiPath aiPath, ExclusiveGroupStruct previousGroup, EGID egid)
        {
            if (GroupTagExtensions.Contains<SUNK>(egid.groupID))
            {
                aiPath.Waypoint = (uint)aiPath.Path.Count<uint>();
            }
        }

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
                CheckPathInvalidation();
                Process();
                yield return null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void CheckPathInvalidation()
        {
            while (_consumer.TryDequeue(out var target, out var egid))
            {
                ref var path = ref entitiesDB.QueryEntity<AiPath>(egid);
                path.Waypoint = (uint)path.Path.Count<uint>();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Process()
        {
            var grid = entitiesDB.QueryEntity<GridComponent>(0, GridGroups.Grid);
            var (navigations, ships, count) = entitiesDB.QueryEntities<ShipNavigationComponent, ShipComponent>(AiGroups.AiShip);
            var (targets, paths, _) = entitiesDB.QueryEntities<AiTarget, AiPath>(AiGroups.AiShip);
            for (var i = 0; i < count; i++)
            {
                var navigation = navigations[i];
                var target = targets[i];
                ref var path = ref paths[i];
                ref var ship = ref ships[i];

                if (path.Waypoint == path.Path.Count<uint>())
                {
                    path.Path.Clear();
                }

                if (path.Path.Count<uint>() == 0)
                {
                    var targetCell = target.Position;
                    path.Waypoint = 0;
                    FindPath(ref path, navigation.GridCell, targetCell, grid);
                }
                else if (path.Path.Get<uint>(path.Waypoint) == _gridUtils.CellToEntityId(navigation.GridCell))
                {
                    path.Waypoint++;
                }

                if (path.Path.Count<uint>() == path.Waypoint) continue;

                var waypoint = path.Path.Get<uint>(path.Waypoint);
                var nextCell = _gridUtils.EntityIdToCell(waypoint);
                if (nextCell.Equals(navigation.GridCell) == false)
                {
                    ship.Direction = new float3(math.sign((int2)nextCell - (int2)navigation.GridCell), 0);

                    // note: this is here to catch a bug where it is possible to have a waypoint that isn't a neighbor.
                        // this might happen in edge cases causing the ship to move diagonally for a few frames.
                    var absDirection = math.abs(ship.Direction);
                    if (absDirection.x > 0 && absDirection.y > 0)
                    {
                        ship.Direction.y = 0;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void FindPath(ref AiPath path, uint2 current, uint2 target, GridComponent grid)
        {
            var cellCount = _gridUtils.GetCellCount();
            var currentCell = _gridUtils.CellToEntityId(current);
            var targetCell = _gridUtils.CellToEntityId(target);
            // Prevent unnecessary calculations when target cell is not in a walkable path.
            if (grid.WalkableGrid.Get<bool>(targetCell) == false) return;
            // Create a* data structures.
            var distance = (uint)math.ceil(math.distance(current, target));
            var visitList = NativeDynamicArray.Alloc<uint>(Allocator.TempJob, distance * 2);
            var visitedList = NativeDynamicArray.Alloc<uint>(Allocator.TempJob, distance * 2);
            var pathGrid = NativeDynamicArray.Alloc<PathNode>(Allocator.TempJob, cellCount);

            visitList.Add(currentCell);
            visitedList.Add(currentCell);
            pathGrid.Set(currentCell, new PathNode { from = currentCell, length = 0 });
            var foundPath = false;
            uint i = 0;
            while (i < visitList.Count<uint>())
            {
                var cell = visitList.Get<uint>(i);
                var pathToCell = pathGrid.Get<PathNode>(cell);
                // todo: add heuristics.
                for (uint direction = 0; direction < 4; direction++)
                {
                    if (_gridUtils.TryGetCellIdInDirection(cell, direction, out var neighbor))
                    {
                        // Only consider walkable cells.
                        if (grid.WalkableGrid.Get<bool>(neighbor) == false) continue;
                        // Have we reach our target?
                        if (neighbor == targetCell)
                        {
                            pathGrid.Set(neighbor, new PathNode { from = cell, length = pathToCell.length + 1});
                            foundPath = true;
                            break;
                        }
                        // Has this cell already been visited?
                        if (IsVisited(visitedList, neighbor) == false)
                        {
                            pathGrid.Set(neighbor, new PathNode { from = cell, length = pathToCell.length + 1});
                            visitList.Add(neighbor);
                            visitedList.Add(neighbor);
                        }
                    }
                }

                if (foundPath) break;

                i++;
            }

            // Build path and get next cell if found.
            if (foundPath)
            {
                var nextCell = targetCell;
                // Make sure we can store the path.
                var pathToCell = pathGrid.Get<PathNode>(targetCell);
                if (pathToCell.length > path.Path.Capacity<uint>())
                {
                    path.Path.Grow<uint>(pathToCell.length);
                }

                while (nextCell != currentCell)
                {
                    path.Path.Set(pathToCell.length - 1, nextCell);
                    nextCell = pathToCell.from;
                    pathToCell = pathGrid.Get<PathNode>(nextCell);
                }
            }
            // Dispose all structures.
            visitList.Dispose();
            visitedList.Dispose();
            pathGrid.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IsVisited(NativeDynamicArray visitedList, uint cell)
        {
            for (uint i = 0; i < visitedList.Count<uint>(); i++)
            {
                var visitedCell = visitedList.Get<uint>(i);
                if (visitedCell == cell) return true;
            }

            return false;
        }
    }

    struct PathNode
    {
        public uint from;
        public uint length;
    }
}