using GreedyMerchants.Data.Grid;
using Svelto.ECS;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using UGrid = UnityEngine.Grid;

namespace GreedyMerchants.ECS.Grid
{
    public class GridUtils
    {
        readonly UGrid _grid;
        readonly Tilemap _landTileMap;
        readonly uint2 _gridSize;
        readonly float2 _cellSize;
        readonly float2 _gridOffset;

        public GridUtils(UGrid grid, Tilemap landTilemap, GridDefinition definition)
        {
            _gridSize = definition.Size;
            _cellSize = definition.CellSize;
            _grid = grid;
            _landTileMap = landTilemap;
            _gridOffset = _gridSize * _cellSize * -0.5f;
        }

        public uint2 GetSize()
        {
            return _gridSize;
        }

        public uint CellToEntityId(uint2 cellPosition)
        {
            if (cellPosition.x >= _gridSize.x || cellPosition.y >= _gridSize.y)
            {
                return cellPosition.x * cellPosition.y;
            }

            return cellPosition.x * _gridSize.x + cellPosition.y;
        }

        public float2 CellToCenterPosition(uint2 cellPosition)
        {
            return new float2(cellPosition.x, cellPosition.y) * _cellSize + _gridOffset + _cellSize * 0.5f;
        }

        public uint2 WorldToCellPosition(float2 worldPosition)
        {
            worldPosition -= _gridOffset;
            worldPosition /= _cellSize;
            math.floor(worldPosition);
            return new uint2((uint)worldPosition.x, (uint)worldPosition.y);
        }

        public bool IsLand(uint2 cellPosition)
        {
            var position = CellToCenterPosition(cellPosition);
            var gridPosition = _grid.WorldToCell(new float3(position.x, position.y, 0));
            return _landTileMap.GetTile(gridPosition) != null;
        }
    }
}