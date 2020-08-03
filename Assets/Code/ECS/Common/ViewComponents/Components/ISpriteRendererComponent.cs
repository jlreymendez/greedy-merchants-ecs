using Unity.Mathematics;
using UnityEngine;

namespace GreedyMerchants.ECS.Common
{
    public interface ISpriteRendererComponent
    {
        bool Render { set; }
        int Sprite { get; set; }
        int Layer { set; }
        int Order { set;  }
        bool2 Flip { set; }
    }
}