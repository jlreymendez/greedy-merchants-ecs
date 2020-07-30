using System.Collections;
using Svelto.ECS;
using Unity.Mathematics;

namespace GreedyMerchants.ECS.Grid.Engines
{
    public class GridSpawningEngine : IQueryingEntitiesEngine
    {
        readonly IEntityFactory _entityFactory;
        readonly GridUtils _gridUtils;

        public GridSpawningEngine(IEntityFactory entityFactory, GridUtils gridUtils)
        {
            _entityFactory = entityFactory;
            _gridUtils = gridUtils;
        }

        public EntitiesDB entitiesDB { get; set; }

        public void Ready()
        {
            CreateGridEntities();
        }

        void CreateGridEntities()
        {
            var landGroup = GridGroups.GridLandGroup.BuildGroup;
            var waterGroup = GridGroups.GridWaterNoCoinGroup.BuildGroup;
            var gridSize = _gridUtils.GetSize();
            for (uint x = 0; x < gridSize.x; x++)
            {
                for (uint y = 0; y < gridSize.y; y++)
                {
                    var cellPosition = new uint2(x, y);
                    var isLand = _gridUtils.IsLand(cellPosition);
                    var initializer = _entityFactory.BuildEntity<GridCellEntityDescriptor>(
                        _gridUtils.CellToEntityId(cellPosition), isLand ? landGroup : waterGroup
                    );

                    initializer.Init(new GridCellComponent {
                        Position = cellPosition, WorldCenter = _gridUtils.CellToCenterPosition(cellPosition)
                    });
                }
            }
        }
    }
}