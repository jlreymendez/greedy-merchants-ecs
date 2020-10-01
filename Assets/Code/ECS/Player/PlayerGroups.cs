using GreedyMerchants.ECS.Extensions.Svelto;
using GreedyMerchants.ECS.Ship;
using Svelto.ECS;

namespace GreedyMerchants.ECS.Player
{
    public static class PlayerGroups
    {
        static PlayerGroups()
        {
            GroupCompound<PLAYER_SHIP, AFLOAT>.BuildGroup.SetTagSwap<AFLOAT, SUNK>(GroupCompound<SUNK, PLAYER_SHIP>.BuildGroup);
        }

        public static readonly ExclusiveGroupStruct PlayerShip = GroupCompound<PLAYER_SHIP, AFLOAT>.BuildGroup;
    }

    public class PLAYER_SHIP : GroupTag<PLAYER_SHIP> {}
}