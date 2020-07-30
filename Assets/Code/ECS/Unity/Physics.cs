using UnityEngine;
using UPhysics = UnityEngine.Physics;

namespace GreedyMerchants.ECS.Unity
{
    public class Physics : IPhysics
    {
        public bool CastRay(Ray ray, out RaycastHit hit, float maxDistance, int layerMask = UPhysics.AllLayers)
        {
            return UPhysics.Raycast(ray, out hit, maxDistance, layerMask);
        }
    }

    public interface IPhysics
    {
        bool CastRay(Ray ray, out RaycastHit hit, float maxDistance, int layerMask);
    }
}