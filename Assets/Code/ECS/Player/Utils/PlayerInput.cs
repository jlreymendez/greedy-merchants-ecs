using Unity.Mathematics;
using UnityEngine;

namespace GreedyMerchants.ECS.Player
{
    public class PlayerInput
    {
        public int2 GetDirection()
        {
            return new int2((int)math.round(Input.GetAxisRaw("Horizontal")), (int)math.round(Input.GetAxisRaw("Vertical")));
        }
    }
}