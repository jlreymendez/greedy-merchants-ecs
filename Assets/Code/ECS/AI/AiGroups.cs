using GreedyMerchants.ECS.Ship;
using Svelto.ECS;

namespace GreedyMerchants.ECS.AI
{
    public class AiGroups
    {
        public static readonly ExclusiveGroupStruct AiShip = GroupCompound<AI_SHIP, AFLOAT>.BuildGroup;
    }

    public class AI_SHIP : GroupTag<AI_SHIP> {}
}