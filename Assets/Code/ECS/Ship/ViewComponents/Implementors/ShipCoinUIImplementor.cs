using System;
using Svelto.ECS.Hybrid;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace GreedyMerchants.ECS.Ship
{
    public class ShipCoinUIImplementor : MonoBehaviour, IImplementor, IShipCoinUIComponent
    {
        Canvas _canvas;
        Text _text;

        public int Coins
        {
            set => _text.text = value.ToString();
        }

        public bool Visibility
        {
            set => _canvas.gameObject.SetActive(value);
        }

        void Awake()
        {
            _canvas = GetComponentInChildren<Canvas>();
            _text = _canvas.GetComponentInChildren<Text>();
            Coins = 0;
        }

        void Update()
        {
            _canvas.transform.right = math.abs(_canvas.transform.right);
        }
    }
}