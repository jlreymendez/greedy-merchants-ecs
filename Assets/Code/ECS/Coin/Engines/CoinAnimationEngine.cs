using System.Collections;
using GreedyMerchants.ECS.Unity;
using Svelto.ECS;
using Svelto.Tasks.Enumerators;
using Unity.Mathematics;

namespace GreedyMerchants.ECS.Coin
{
    public class CoinAnimationEngine : IQueryingEntitiesEngine
    {
        ITime _time;
        WaitForSecondsEnumerator _frameWait;

        const float SecondsPerFrame = 1f / 10f;
        const int FrameCount = 6;

        public CoinAnimationEngine(ITime time)
        {
            _time = time;
        }


        public EntitiesDB entitiesDB { get; set; }

        public void Ready()
        {
            Tick().Run();
        }

        IEnumerator Tick()
        {
            _frameWait = new WaitForSecondsEnumerator(SecondsPerFrame);

            while (true)
            {
                yield return _frameWait;
                Process();
            }
        }

        void Process()
        {
            var (coinViews, count) = entitiesDB.QueryEntities<CoinViewComponent>(CoinGroups.SpawnedCoinsGroup);

            for (var i = 0; i < count; i++)
            {
                ref var coin = ref coinViews[i];
                coin.Renderer.Sprite = (coin.Renderer.Sprite + 1) % FrameCount;
                coin.Renderer.Flip = new bool2(coin.Renderer.Sprite > 3, false);
            }
        }
    }
}