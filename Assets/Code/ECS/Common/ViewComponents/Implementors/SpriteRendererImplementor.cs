using System.Collections;
using Svelto.ECS.Hybrid;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GreedyMerchants.ECS.Common
{
    public class SpriteRendererImplementor : MonoBehaviour, IImplementor, ISpriteRendererComponent
    {
        public Sprite[] Sprites;
        SpriteRenderer _spriteRenderer;

        public int Sprite { set => _spriteRenderer.sprite = Sprites[value]; }

        public bool Render { set => _spriteRenderer.enabled = value; }

        public int Layer { set => _spriteRenderer.sortingLayerID = value; }

        public int Order { set => _spriteRenderer.sortingOrder = value; }

        void Awake()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }
}