using System.Collections;
using GreedyMerchants.ECS.Extensions.Svelto;
using GreedyMerchants.ECS.Player;
using GreedyMerchants.ECS.Ship;
using Svelto.ECS;

namespace GreedyMerchants.ECS.AI
{
    public class ShipAIStateEngine : IQueryingEntitiesEngine
    {
        IEntityFunctions _functions;
        Consumer<ShipLevelComponent> _consumer;

        public ShipAIStateEngine(IEntityFunctions functions, IEntityStreamConsumerFactory consumerFactory)
        {
            _functions = functions;
            _consumer = consumerFactory.GenerateConsumer<ShipLevelComponent>("ShipAIState", 4);
        }

        public EntitiesDB entitiesDB { get; set; }

        public void Ready()
        {
            GroupCompound<SHIP, Ship.AI, MERCHANT>.BuildGroup.SetTagSwap<MERCHANT, NORMAL>(GroupCompound<SHIP, Ship.AI, NORMAL>.BuildGroup);
            GroupCompound<SHIP, Ship.AI, MERCHANT>.BuildGroup.SetTagSwap<MERCHANT, PIRATE>(GroupCompound<SHIP, Ship.AI, PIRATE>.BuildGroup);
            GroupCompound<SHIP, Ship.AI, MERCHANT>.BuildGroup.SetTagSwap<NORMAL, PIRATE>(GroupCompound<SHIP, Ship.AI, PIRATE>.BuildGroup);

            GroupCompound<SUNK_SHIP, Ship.AI, MERCHANT>.BuildGroup.SetTagSwap<MERCHANT, NORMAL>(GroupCompound<SUNK_SHIP, Ship.AI, NORMAL>.BuildGroup);
            GroupCompound<SUNK_SHIP, Ship.AI, MERCHANT>.BuildGroup.SetTagSwap<MERCHANT, PIRATE>(GroupCompound<SUNK_SHIP, Ship.AI, PIRATE>.BuildGroup);
            GroupCompound<SUNK_SHIP, Ship.AI, MERCHANT>.BuildGroup.SetTagSwap<NORMAL, PIRATE>(GroupCompound<SUNK_SHIP, Ship.AI, PIRATE>.BuildGroup);

            Tick().Run();
        }

        IEnumerator Tick()
        {
            while (true)
            {
                while (_consumer.TryDequeue(out ShipLevelComponent shipLevel, out EGID egid))
                {
                    // note: Maybe it is better to add this as data, since the group tag contains check is potentially expensive.
                    if (GroupTagExtensions.Contains<Ship.AI>(egid.groupID))
                    {
                        var targetGroup = egid.groupID;
                        switch (shipLevel.Level)
                        {
                            case ShipLevel.Merchant:
                                targetGroup = egid.groupID.SwapTag<MERCHANT>();
                                break;
                            case ShipLevel.Pirate:
                                targetGroup = egid.groupID.SwapTag<PIRATE>();
                                break;
                            case ShipLevel.Normal:
                                targetGroup = egid.groupID.SwapTag<NORMAL>();
                                break;
                        }

                        if (targetGroup != egid.groupID)
                        {
                            _functions.SwapEntityGroup<ShipEntityDescriptor>(egid, targetGroup);
                        }
                    }
                }
            }
        }
    }
}