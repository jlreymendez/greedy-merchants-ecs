using Unity.Mathematics;
using UnityEngine;

namespace GreedyMerchants.ECS.Player
{
    public class PlayerInput
    {
        public float3 GetDirection()
        {
            return new float3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0);
        }
    }
}