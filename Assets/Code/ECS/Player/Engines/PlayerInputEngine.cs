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
                var (ships, count) = entitiesDB.QueryEntities<ShipComponent>(GameGroups.PlayerShip);
                for (var i = 0; i < count; i++)
                {
                    var direction = _input.GetDirection();
                    if (direction.x != 0)
                    {
                        ships[i].Direction = new float3(direction.x, 0, 0);
                    }

                    if (direction.y != 0)
                    {
                        ships[i].Direction = new float3(0, direction.y, 0);
                    }
                }

                yield return null;
            }
        }
    }
}