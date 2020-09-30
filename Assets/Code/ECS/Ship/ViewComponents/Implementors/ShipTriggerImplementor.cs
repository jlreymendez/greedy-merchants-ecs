using Svelto.ECS;
using Svelto.ECS.Extensions.Unity;
using Svelto.ECS.Hybrid;
using UnityEngine;

namespace GreedyMerchants.ECS.Ship
{
    public class ShipTriggerImplementor : MonoBehaviour, IImplementor, IShipTriggerComponent
    {
        public DispatchOnSet<ShipCollisionData> HitChange { get; set; }

        public void OnTriggerEnter2D(Collider2D other)
        {
            CheckCollision(other);
        }

        public void OnTriggerStay2D(Collider2D other)
        {
            CheckCollision(other);
        }

        void CheckCollision(Collider2D other)
        {
            if (HitChange == null) return;

            var egidHolder = other.gameObject.GetComponent<EGIDHolderImplementor>();
            if (egidHolder != null)
            {
                HitChange.value = new ShipCollisionData(egidHolder.ID, other.gameObject.layer, Time.time);
            }
        }
    }
}