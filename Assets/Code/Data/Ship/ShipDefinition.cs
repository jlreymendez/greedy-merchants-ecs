using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

namespace GreedyMerchants.Data.Ship
{
    [CreateAssetMenu(fileName = "Ship Defintion", menuName = "Game/Ship", order = 0)]
    public class ShipDefinition : ScriptableObject
    {
        public AssetReference Prefab;

        public float Speed;
        public float PirateSpeed;
        public float MerchantSpeed;

        [FormerlySerializedAs("PlayerSprites")] public AssetReference[] ShipSprites;
        public AssetReference PirateSprite;
        public AssetReference MerchantSprite;
    }
}