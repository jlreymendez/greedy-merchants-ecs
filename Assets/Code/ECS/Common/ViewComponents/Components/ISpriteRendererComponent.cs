using UnityEngine;

namespace GreedyMerchants.ECS.Common
{
    public interface ISpriteRendererComponent
    {
        bool Render { set; }
        int Sprite { set; }
        int Layer { set; }
        int Order { set;  }
    }
}