using GreedyMerchants.Data.Grid;
using GreedyMerchants.Data.Ship;
using Svelto.Context;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GreedyMerchants
{
    public class GameContext : UnityContext<GameCompositionRoot>
    {
        [Header("Ship")]
        public ShipDefinition Ship;
        public Transform[] ShipSpawns;

        [Header("Grid")]
        public Grid Grid;
        public Tilemap LandTilemap;
        public GridDefinition GridDefinition;
    }
}