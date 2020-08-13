using System.Runtime.CompilerServices;
using GreedyMerchants.Data.Grid;
using Unity.Mathematics;
using UnityEngine.Tilemaps;
using UGrid = UnityEngine.Grid;

namespace GreedyMerchants.ECS.Grid
{
    public class GridUtils
    {
        readonly uint2 _gridSize;
        readonly float2 _cellSize;
        readonly float2 _gridOffset;

        public GridUtils(GridDefinition definition)
        {
            _gridSize = definition.Size;
            _cellSize = definition.CellSize;
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
        public uint2 EntityIdToCell(uint cellId)
        {
            return new uint2(cellId % _gridSize.x, cellId / _gridSize.x);
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
        public uint GetCellCount()
        {
            return _gridSize.x * _gridSize.y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetCellIdInDirection(uint cellId, GridDirection direction, out uint neighbor)
        {
            neighbor = 0;
            switch (direction)
            {
                case GridDirection.Up:
                    return TryGetUpCellId(cellId, out neighbor);
                case GridDirection.Right:
                    return TryGetRightCellId(cellId, out neighbor);
                case GridDirection.Down:
                    return TryGetDownCellId(cellId, out neighbor);
                case GridDirection.Left:
                    return TryGetLeftCellId(cellId, out neighbor);
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetUpCellId(uint cellId, out uint upCellId)
        {
            upCellId = 0;
            if (cellId >= GetCellCount() - _gridSize.x) return false;
            upCellId = cellId + _gridSize.x;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetDownCellId(uint cellId, out uint upCellId)
        {
            upCellId = 0;
            if (cellId < _gridSize.x) return false;
            upCellId = cellId - _gridSize.x;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetRightCellId(uint cellId, out uint upCellId)
        {
            upCellId = 0;
            if ((cellId % _gridSize.x) == _gridSize.x - 1) return false;
            upCellId = cellId + 1;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetLeftCellId(uint cellId, out uint upCellId)
        {
            upCellId = 0;
            if ((cellId % _gridSize.x) == 0) return false;
            upCellId = cellId - 1;
            return true;
        }
    }
}