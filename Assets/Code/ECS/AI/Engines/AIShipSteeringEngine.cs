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
            var currentCell = _gridUtils.CellToEntityId(current);
            var targetCell = _gridUtils.CellToEntityId(target);
            // Prevent unnecessary calculations when target cell is not in a walkable path.
            if (grid.WalkableGrid.Get<bool>(targetCell) == false) return;

            var distance = (uint)math.ceil(math.distance(current, target));
            var cellCount = _gridUtils.GetCellCount();
            var pathGrid = NativeDynamicArray.Alloc<PathNode>(Allocator.Temp, cellCount);

            // Build path and get next cell if found.
            if (AStarPath(grid, pathGrid, currentCell, targetCell, distance))
            {
                BuildPath(ref path, pathGrid, currentCell, targetCell);
            }

            pathGrid.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool AStarPath(GridComponent grid, NativeDynamicArray pathGrid, uint current, uint target, uint distance)
        {
            // Create a* data structures.
            var visitList = NativeDynamicArray.Alloc<uint>(Allocator.Temp, distance * 2);
            var visitedList = NativeDynamicArray.Alloc<uint>(Allocator.Temp, distance * 2);

            var targetPosition = _gridUtils.EntityIdToCell(target);

            visitList.Add(current);
            visitedList.Add(current);
            pathGrid.Set(current, new PathNode { from = current, length = 0 });
            var foundPath = false;
            uint i = 0;
            while (i < visitList.Count<uint>())
            {
                var cell = visitList.Get<uint>(i);
                var cellPosition = _gridUtils.EntityIdToCell(cell);
                var pathToCell = pathGrid.Get<PathNode>(cell);

                var heuristics = BuildHeuristics(cellPosition, targetPosition);
                while (heuristics.MoveNext())
                {
                    if (_gridUtils.TryGetCellIdInDirection(cell, heuristics.Current, out var neighbor))
                    {
                        // Only consider walkable cells.
                        if (grid.WalkableGrid.Get<bool>(neighbor) == false) continue;
                        // Have we reach our target?
                        if (neighbor == target)
                        {
                            pathGrid.Set(neighbor, new PathNode(cell, pathToCell.length + 1));
                            foundPath = true;
                            break;
                        }
                        // Has this cell already been visited?
                        if (IsVisited(visitedList, neighbor) == false)
                        {
                            pathGrid.Set(neighbor, new PathNode(cell, pathToCell.length + 1));
                            visitList.Add(neighbor);
                            visitedList.Add(neighbor);
                        }
                    }
                }

                if (foundPath) break;

                i++;
            }

            visitList.Dispose();
            visitedList.Dispose();

            return foundPath;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        PathHeuristics BuildHeuristics(uint2 current, uint2 target)
        {
            PathHeuristics heuristics;
            var direction = (int2) (target - current);
            if (direction.x > 0 && direction.y >= 0)
            {
                if (direction.y >= 0)
                {
                    heuristics = new PathHeuristics(GridDirection.Right, false);
                }
                else
                {
                    heuristics = new PathHeuristics(GridDirection.Right, true);
                }
            }
            else if (direction.x < 0)
            {
                if (direction.y >= 0)
                {
                    heuristics = new PathHeuristics(GridDirection.Left, true);
                }
                else
                {
                    heuristics = new PathHeuristics(GridDirection.Left, false);
                }
            }
            else if (direction.y > 0)
            {
                if (direction.x >= 0)
                {
                    heuristics = new PathHeuristics(GridDirection.Up, true);
                }
                else
                {
                    heuristics = new PathHeuristics(GridDirection.Up, false);
                }
            }
            else
            {
                if (direction.x >= 0)
                {
                    heuristics = new PathHeuristics(GridDirection.Down, false);
                }
                else
                {
                    heuristics = new PathHeuristics(GridDirection.Down, true);
                }
            }

            return heuristics;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void BuildPath(ref AiPath path, NativeDynamicArray pathGrid, uint current, uint target)
        {
            var nextCell = target;
            // Make sure we can store the path.
            var pathToCell = pathGrid.Get<PathNode>(target);
            if (pathToCell.length > path.Path.Capacity<uint>())
            {
                path.Path.Grow<uint>(pathToCell.length);
            }

            while (nextCell != current)
            {
                path.Path.Set(pathToCell.length - 1, nextCell);
                nextCell = pathToCell.from;
                pathToCell = pathGrid.Get<PathNode>(nextCell);
            }
        }
    }
}