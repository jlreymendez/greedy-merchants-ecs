using GreedyMerchants.ECS.Extensions.Svelto;
using Svelto.ECS;

namespace GreedyMerchants.ECS.Ship
{
    public class ShipSinkingEngine : IQueryingEntitiesEngine, IReactOnSwap<ShipViewComponent>
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        public void MovedTo(ref ShipViewComponent entityComponent, ExclusiveGroupStruct previousGroup, EGID egid)
        {
            if (GroupTagExtensions.Contains<SUNK_SHIP>(egid.groupID))
            {
                entityComponent.Renderer.Render = false;
                entityComponent.Physics.Enable = false;
                entityComponent.Explosion.Play = true;
            }
            else
            {
                entityComponent.Explosion.Play = false;
            }
        }
    }
}