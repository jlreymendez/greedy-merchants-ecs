using Svelto.ECS.Hybrid;
using UnityEngine;
using UnityEngine.UI;

namespace GreedyMerchants.ECS.HUD
{
    public class HUDPointsImplementor : MonoBehaviour, IImplementor, IPointViewComponent
    {
        Text _text;

        public int Points { set => _text.text = value.ToString(); }

        void Awake()
        {
            _text = GetComponentInChildren<Text>();
        }
    }
}