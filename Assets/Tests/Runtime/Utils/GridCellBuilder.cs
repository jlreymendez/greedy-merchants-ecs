using GreedyMerchants.ECS.Grid;
using Svelto.ECS;
using Unity.Mathematics;

namespace GreedyMerchants.Tests.Runtime.Utils
{
    public class GridCellBuilder
    {

        public EGID Build()
        {
            var initializer = The.Context.EntityFactory.BuildEntity<GridCellEntityDescriptor>(_id, _group);

            var cellId = The.Grid.Utils.EntityIdToCell(_id);
            initializer.Init(new GridCellComponent
            {
                Position = cellId,
                WorldCenter =  The.Grid.Utils.CellToCenterPosition(cellId)
            });

            return initializer.EGID;
        }

        public GridCellBuilder WithPosition(float2 position)
        {
            _id = The.Grid.Utils.WorldToEnitityId(position);
            _position = position;
            return this;
        }

        public GridCellBuilder WithGroup(ExclusiveGroup group)
        {
            _group = group;
            return this;
        }

        uint _id;
        ExclusiveGroupStruct _group = GridGroups.GridWaterGroup;
        float2 _position;

        public static implicit operator EGID(GridCellBuilder builder)
        {
            return builder.Build();
        }
    }
}