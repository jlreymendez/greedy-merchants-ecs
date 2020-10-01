using GreedyMerchants.ECS.Extensions.Svelto;
using GreedyMerchants.ECS.Ship;
using Svelto.ECS;

namespace GreedyMerchants.ECS.AI
{
    public static class AiGroups
    {
        static AiGroups()
        {
            GroupCompound<AI_SHIP, AFLOAT>.BuildGroup.SetTagSwap<AFLOAT, SUNK>(GroupCompound<SUNK, AI_SHIP>.BuildGroup);
        }

        public static readonly ExclusiveGroupStruct AiShip = GroupCompound<AI_SHIP, AFLOAT>.BuildGroup;
    }

    public class AI_SHIP : GroupTag<AI_SHIP> {}
}