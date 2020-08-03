using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Code.Data.Coin
{
    [CreateAssetMenu(menuName = "Game/Coin", fileName = "CoinDefinition")]
    public class CoinDefinition : ScriptableObject
    {
        public AssetReference Prefab;
        public int MaxCoins;
        public float CoinsSpawnTime;
    }
}