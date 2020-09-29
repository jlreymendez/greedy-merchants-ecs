using Svelto.ECS;
using Svelto.ECS.Hybrid;

namespace GreedyMerchants.ECS.HUD
{
    public class HUDPointsViewComponent : IEntityViewComponent
    {
        public IPointViewComponent Points;

        public EGID ID { get; set; }
    }
}