using Svelto.ECS.Hybrid;
using UnityEngine;

namespace GreedyMerchants.ECS.Common
{
    public class Physics2DImplementor : MonoBehaviour, IImplementor, IPhysicsComponent
    {
        Collider2D[] _colliders;

        public bool Enable
        {
            set
            {
                foreach (var collider in _colliders)
                {
                    collider.enabled = value;
                }
            }
        }

        void Awake()
        {
            _colliders = GetComponentsInChildren<Collider2D>(true);
        }
    }
}