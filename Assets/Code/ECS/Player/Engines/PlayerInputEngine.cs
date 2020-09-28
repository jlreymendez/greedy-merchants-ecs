using System.Collections;
using GreedyMerchants.ECS.Ship;
using Svelto.ECS;
using Unity.Mathematics;

namespace GreedyMerchants.ECS.Player
{
    public class PlayerInputEngine : IQueryingEntitiesEngine
    {
        PlayerInput _input;

        public PlayerInputEngine(PlayerInput input)
        {
            _input = input;
        }

        public EntitiesDB entitiesDB { get; set; }

        public void Ready()
        {
            Tick().Run();
        }

        IEnumerator Tick()
        {
            while (true)
            {
                var (ships, count) = entitiesDB.QueryEntities<ShipComponent>(PlayerGroups.PlayerShip);
                for (var i = 0; i < count; i++)
                {
                    // todo: this doesn't handle multiple input controllers.
                    var direction = _input.GetDirection();
                    // Limit user input to one axis.
                    direction = direction.x != 0 ? new int2(direction.x, 0) : new int2(0, direction.y);
                    // Ship should never have int2.zero as its direction
                    if (math.length(direction) > 0)
                    {
                        ships[i].Direction = direction;
                    }
                }

                yield return null;
            }
        }
    }
}