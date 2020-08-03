using Code.Data.Coin;
using GreedyMerchants.Data.Grid;
using GreedyMerchants.Data.Ship;
using Svelto.Context;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GreedyMerchants
{
    public class GameContext : UnityContext<GameCompositionRoot>
    {
        [Header("Random")]
        public uint Seed;

        [Header("Ship")]
        public ShipDefinition Ship;
        public Transform[] ShipSpawns;

        [Header("Grid")]
        public GridDefinition GridDefinition;
        public Grid Grid;
        public Tilemap LandTilemap;

        [Header("Coin")] public CoinDefinition CoinDefinition;
    }
}