using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine.Tilemaps;
using UGrid = UnityEngine.Grid;

namespace GreedyMerchants.ECS.Grid
{
    public class GridTilemapRepresentation
    {
        readonly UGrid _grid;
        readonly Tilemap _landTilemap;
        GridUtils _utils;

        public GridTilemapRepresentation(UGrid grid, Tilemap landTilemap, GridUtils utils)
        {
            _grid = grid;
            _landTilemap = landTilemap;
            _utils = utils;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsLand(uint2 cellPosition)
        {
            var position = _utils.CellToCenterPosition(cellPosition);
            var gridPosition = _grid.WorldToCell(new float3(position.x, position.y, 0));
            return _landTilemap.GetTile(gridPosition) != null;
        }
    }
}