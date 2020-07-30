using GreedyMerchants.Data.Ship;
using UnityEngine;

namespace GreedyMerchants.Data
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Config", order = 0)]
    public class GameConfig : ScriptableObject
    {
        public ShipControl[] ShipControls;
    }
}