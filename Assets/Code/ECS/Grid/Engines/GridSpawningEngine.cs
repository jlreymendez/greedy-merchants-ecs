using System;
using Svelto.Common;
using Svelto.ECS;
using Svelto.ECS.DataStructures;
using Unity.Mathematics;

namespace GreedyMerchants.ECS.Grid.Engines
{
    public class GridSpawningEngine : IQueryingEntitiesEngine, IDisposable
    {
        readonly IEntityFactory _entityFactory;
        readonly GridUtils _gridUtils;
        readonly GridTilemapRepresentation _gridLand;

        public GridSpawningEngine(IEntityFactory entityFactory, GridUtils gridUtils, GridTilemapRepresentation gridLand)
        {
            _entityFactory = entityFactory;
            _gridUtils = gridUtils;
            _gridLand = gridLand;
        }

        public EntitiesDB entitiesDB { get; set; }

        public void Ready()
        {
            CreateGridEntities();
        }

        void CreateGridEntities()
        {
            var gridSize = _gridUtils.GetSize();
            var gridInitializer = _entityFactory.BuildEntity<GridEntityDescriptor>(0, GridGroups.Grid);
            var walkableGrid = NativeDynamicArray.Alloc<bool>(Allocator.Persistent, gridSize.x * gridSize.y);

            for (uint x = 0; x < gridSize.x; x++)
            {
                for (uint y = 0; y < gridSize.y; y++)
                {
                    var cellPosition = new uint2(x, y);
                    var cellIndex = _gridUtils.CellToEntityId(cellPosition);
                    var isLand = _gridLand.IsLand(cellPosition);
                    // note: it is probable that we don't need to create the land cells at all,
                        // unless there is some gameplay feature that needs them.
                    var initializer = _entityFactory.BuildEntity<GridCellEntityDescriptor>(
                        cellIndex, isLand ? GridGroups.GridLandGroup : GridGroups.GridWaterGroup
                    );

                    initializer.Init(new GridCellComponent {
                        Position = cellPosition, WorldCenter = _gridUtils.CellToCenterPosition(cellPosition)
                    });

                    walkableGrid.Set(cellIndex, !isLand);
                }
            }

            gridInitializer.Init(new GridComponent { WalkableGrid = walkableGrid });
        }

        public void Dispose()
        {
            var grid = entitiesDB.QueryEntity<GridComponent>(0, GridGroups.Grid);
            grid.WalkableGrid.Dispose();
        }
    }
}