
using GreedyMerchants.Data.Ship;
using Svelto.ECS.Hybrid;
using UnityEngine;
using UnityEngine.UI;

namespace GreedyMerchants.ECS.Ship
{
    public class ShipPointsHudImplementor : MonoBehaviour, IImplementor, IShipPointsHudComponent
    {
        GameObject[] _huds;
        Image _icon;
        Text _text;

        public ShipColor Color
        {
            set
            {
                var index = (int) value;
                if (_huds.Length <= index)
                {
                    _icon = null;
                    _text = null;
                }
                else
                {
                    _icon = _huds[index].GetComponentInChildren<Image>();
                    _text = _huds[index].GetComponentInChildren<Text>();
                }
            }
        }

        public int Points
        {
            set
            {
                if (_text != null) _text.text = value.ToString();
            }
        }

        void Start()
        {
            _huds = GameObject.FindGameObjectsWithTag(PointHUDTag);
        }

        const string PointHUDTag = "PointsHUD";
    }
}