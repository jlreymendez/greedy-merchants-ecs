
using GreedyMerchants.Data.Ship;
using Svelto.ECS.Hybrid;
using UnityEngine;
using UnityEngine.UI;

namespace GreedyMerchants.ECS.Ship
{
    public class ShipPointsHudImplementor : MonoBehaviour, IImplementor, IShipPointsHudComponent
    {
        GameObject _huds;
        Text[] _texts = new Text[0];
        Text _text;
        int _index;

        public ShipColor Color
        {
            set
            {
                if (_huds == null)
                {
                    _huds = GameObject.FindGameObjectWithTag(PointHUDTag);
                    if (_huds) _texts = _huds.GetComponentsInChildren<Text>();
                }

                var index = (int) value;
                if (_texts.Length <= index)
                {
                    _text = null;
                }
                else
                {
                    _text = _texts[index].GetComponentInChildren<Text>();
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

        const string PointHUDTag = "PointsHUD";
    }
}