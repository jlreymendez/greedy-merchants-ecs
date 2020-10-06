using System.Collections;
using GreedyMerchants.Data.Audio;
using GreedyMerchants.ECS.AI;
using GreedyMerchants.ECS.Extensions.Svelto;
using GreedyMerchants.ECS.Player;
using GreedyMerchants.Unity;
using Svelto.ECS;

namespace GreedyMerchants.ECS.Ship
{
    public class ShipSinkingEngine : IQueryingEntitiesEngine, IReactOnSwap<ShipViewComponent>, IReactOnSwap<ShipLevelComponent>, ITickingEngine
    {
        IEntityFunctions _functions;

        public ShipSinkingEngine(IEntityFunctions functions)
        {
            _functions = functions;
        }

        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        public void MovedTo(ref ShipViewComponent entityComponent, ExclusiveGroupStruct previousGroup, EGID egid)
        {
            if (GroupTagExtensions.Contains<SUNK>(egid.groupID))
            {
                entityComponent.Renderer.Render = false;
                entityComponent.Renderer.Sprite = (int) ShipLevel.Normal;
                entityComponent.Physics.Enable = false;
                entityComponent.Explosion.Play = true;
                entityComponent.Audio.PlayOneShot = ShipAudioType.Sink;
                entityComponent.CoinsHud.Visibility = false;
            }
            else
            {
                entityComponent.Explosion.Play = false;
            }
        }

        public void MovedTo(ref ShipLevelComponent level, ExclusiveGroupStruct previousGroup, EGID egid)
        {
            if (GroupTagExtensions.Contains<SUNK>(egid.groupID))
            {
                level.Level = ShipLevel.Normal;
                level.NextLevel = ShipLevel.Normal;
            }
        }

        public GameTickScheduler tickScheduler => GameTickScheduler.Late;
        public int Order => (int)GameEngineOrder.Output;

        public IEnumerator Tick()
        {
            while (true)
            {
                Process();
                yield return null;
            }

            void Process()
            {
                var query = entitiesDB.QueryEntities<ShipComponent>(ShipGroups.AliveShipGroups);
                foreach (var ((ships, count), group) in query.groups)
                {
                    for (var i = 0; i < count; i++)
                    {
                        ref var ship = ref ships[i];
                        if (!ship.IsSinking) continue;

                        ship.IsSinking = false;
                        // note: This semi abstracted engine shouldn't know which descriptor it is changing.
                        var targetGroup = ship.ID.groupID.SwapTag<SUNK>();
                        if (GroupTagExtensions.Contains<PLAYER_SHIP>(ship.ID.groupID))
                        {
                            _functions.SwapEntityGroup<PlayerShipDescriptor>(ship.ID, targetGroup);
                        }
                        else
                        {
                            _functions.SwapEntityGroup<AiShipDescriptor>(ship.ID, targetGroup);
                        }
                    }
                }
            }
        }
    }
}