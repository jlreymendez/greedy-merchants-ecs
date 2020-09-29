using Svelto.ECS.Hybrid;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace GreedyMerchants.ECS.Match
{
    public class TimerHudImplementor : MonoBehaviour, IImplementor, ITimerHudComponent
    {
        public Text _text;

        public float Time { set => _text.text = string.Format("{0}", (int)math.ceil(value)); }

        void Awake()
        {
            _text = GetComponentInChildren<Text>();
        }
    }
}