using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GreedyMerchants.Data.Ship
{
    [CreateAssetMenu(fileName = "Ship Defintion", menuName = "Game/Ship", order = 0)]
    public class ShipDefinition : ScriptableObject
    {
        public AssetReference Prefab;

        public float Speed;
        public float PirateSpeed;
        public float MerchantSpeed;

        public AssetReference[] ShipSprites;
        public AssetReference PirateSprite;
        public AssetReference MerchantSprite;

        public float TimeBetweenConversion;
        public float ConversionTransitionTime;

        public float TimeToRespawn;
        public float RespawnTransitionTime;

        public float BlinkAnimationTime = 0.15f;
    }
}