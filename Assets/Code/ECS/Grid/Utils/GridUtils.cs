using System.Runtime.CompilerServices;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint2 GetSize()
        {
            return _gridSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint CellToEntityId(uint2 cellPosition)
        {
            if (cellPosition.x >= _gridSize.x || cellPosition.y >= _gridSize.y)
            {
                return cellPosition.x * cellPosition.y;
            }

            return cellPosition.x + cellPosition.y * _gridSize.x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float2 CellToCenterPosition(uint2 cellPosition)
        {
            return new float2(cellPosition.x, cellPosition.y) * _cellSize + _gridOffset + _cellSize * 0.5f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint2 WorldToCellPosition(float2 worldPosition)
        {
            worldPosition -= _gridOffset;
            worldPosition /= _cellSize;
            math.floor(worldPosition);
            return new uint2((uint)worldPosition.x, (uint)worldPosition.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint WorldToEnitityId(float2 worldPosition)
        {
            var cellPosition = WorldToCellPosition(worldPosition);
            return CellToEntityId(cellPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetUpCellId(uint2 cellPosition, out uint upCellId)
        {
            upCellId = 0;
            if (cellPosition.y == _gridSize.y - 1) return false;
            upCellId = CellToEntityId(cellPosition) + _gridSize.x;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetDownCellId(uint2 cellPosition, out uint upCellId)
        {
            upCellId = 0;
            if (cellPosition.y == 0) return false;
            upCellId = CellToEntityId(cellPosition) - _gridSize.x;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetRightCellId(uint2 cellPosition, out uint upCellId)
        {
            upCellId = 0;
            if (cellPosition.x == _gridSize.x - 1) return false;
            upCellId = CellToEntityId(cellPosition) + 1;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetLeftCellId(uint2 cellPosition, out uint upCellId)
        {
            upCellId = 0;
            if (cellPosition.x == 0) return false;
            upCellId = CellToEntityId(cellPosition) - 1;
            return true;
        }
    }
}