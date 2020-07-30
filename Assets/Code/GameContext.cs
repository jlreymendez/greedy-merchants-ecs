using GreedyMerchants.Data.Ship;
using Svelto.Context;
using UnityEngine;

namespace GreedyMerchants
{
    public class GameContext : UnityContext<GameCompositionRoot>
    {
        public ShipDefinition Ship;
        public Transform[] ShipSpawns;
    }
}