using Svelto.Common.Internal;
using Svelto.ECS.Hybrid;
using Unity.Mathematics;
using UnityEngine;

namespace GreedyMerchants.ECS.Common
{
    public class SpriteRendererImplementor : MonoBehaviour, IImplementor, ISpriteRendererComponent
    {
        public Sprite[] Sprites;
        SpriteRenderer _spriteRenderer;
        int _spriteIndex;

        public int Sprite
        {
            get => _spriteIndex;
            set
            {
                _spriteIndex = math.min(Sprites.Length - 1, value);
                if (_spriteIndex >= 0) _spriteRenderer.sprite = Sprites[value];
            }
        }

        public bool Render { set => _spriteRenderer.enabled = value; }

        public int Layer { set => _spriteRenderer.sortingLayerID = value; }

        public int Order { set => _spriteRenderer.sortingOrder = value; }

        public bool2 Flip
        {
            set
            {
                _spriteRenderer.flipX = value.x;
                _spriteRenderer.flipY = value.y;
            }
        }

        void Awake()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }
}