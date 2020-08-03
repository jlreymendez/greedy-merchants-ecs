using GreedyMerchants.Data.Ship;
using UnityEngine;

namespace GreedyMerchants.Data
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Config", order = 0)]
    public class GameConfig : ScriptableObject
    {
        [SerializeField] uint RandomSeed;

        public ShipControl[] ShipControls;

        [Header("Coins")]
        public int MaxCoins;
        public float CoinsSpawnTime;

        public uint Seed => RandomSeed == 0 ? (uint)Random.Range(int.MinValue, int.MaxValue) : RandomSeed;
    }
}