using Code.Data.Coin;
using GreedyMerchants.Data.Grid;
using GreedyMerchants.Data.Ship;
using Svelto.Context;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace GreedyMerchants
{
    public class GameContext : UnityContext<GameCompositionRoot>
    {
        [Header("Random")]
        public uint Seed;

        [Header("Game Config")]
        public int PointsPerCoin;
        public int PointsPerKill;

        [Header("Ship")]
        public ShipDefinition Ship;
        public Transform[] ShipSpawns;

        [Header("Grid")]
        public GridDefinition GridDefinition;
        public Grid Grid;
        public Tilemap LandTilemap;

        [Header("Coin")] public CoinDefinition CoinDefinition;

        [Header("HUD")] public AssetReference PointsHUDCanvas;
    }
}