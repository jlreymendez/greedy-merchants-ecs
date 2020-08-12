using GreedyMerchants.ECS.Ship;
using Svelto.ECS;

namespace GreedyMerchants.ECS.Player
{
    public class PlayerGroups
    {
        public static readonly ExclusiveGroupStruct PlayerShip = GroupCompound<PLAYER_SHIP, AFLOAT>.BuildGroup;
    }

    public class PLAYER_SHIP : GroupTag<PLAYER_SHIP> {}
}