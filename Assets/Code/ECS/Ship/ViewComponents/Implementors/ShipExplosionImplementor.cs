using GreedyMerchants.ECS.Unity;
using Svelto.ECS.Hybrid;
using UnityEngine;

namespace GreedyMerchants.ECS.Ship
{
    public class ShipExplosionImplementor : MonoBehaviour, IImplementor, IShipExplosionComponent
    {
        public SpriteFx ExplosionFx;

        public bool Play
        {
            set
            {
                if (value)
                {
                    ExplosionFx.Play();
                }
                else
                {
                    ExplosionFx.Stop();
                }
            }
        }
    }
}