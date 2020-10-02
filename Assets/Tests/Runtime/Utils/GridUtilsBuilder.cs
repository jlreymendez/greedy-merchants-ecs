using GreedyMerchants.ECS.Grid;
using Unity.Mathematics;

namespace GreedyMerchants.Tests.Runtime.Utils
{
    public class GridUtilsBuilder
    {
        uint2 _size = new uint2(30, 18);
        float2 _cellSize = new float2(1);

        public GridUtils Build()
        {
            return new GridUtils(_size, _cellSize);
        }

        public GridUtilsBuilder WithSize(uint2 size)
        {
            _size = size;
            return this;
        }

        public GridUtilsBuilder WithCellSize(float2 size)
        {
            _cellSize = size;
            return this;
        }

        public static implicit operator GridUtils(GridUtilsBuilder builder)
        {
            return builder.Build();
        }
    }
}