using System.Collections;
using GreedyMerchants.Data.Audio;
using GreedyMerchants.Data.Ship;
using GreedyMerchants.ECS.Extensions.Svelto;
using GreedyMerchants.Unity;
using Svelto.ECS;
using Svelto.Tasks.Enumerators;

namespace GreedyMerchants.ECS.Ship
{
    public class ShipLevelConversionEngine : IQueryingEntitiesEngine, ITickingEngine
    {
        readonly float[] _speedsByLevel;

        WaitForSecondsEnumerator _conversionWait;
        WaitForSecondsEnumerator _transitionWait;
        WaitForSecondsEnumerator _animationWait;

        public ShipLevelConversionEngine(ShipDefinition shipDefinition)
        {
            _speedsByLevel = new[] { shipDefinition.MerchantSpeed, shipDefinition.Speed, shipDefinition.PirateSpeed };

            _conversionWait = new WaitForSecondsEnumerator(shipDefinition.TimeBetweenConversion);
            _transitionWait = new WaitForSecondsEnumerator(shipDefinition.ConversionTransitionTime);
            _animationWait = new WaitForSecondsEnumerator(shipDefinition.BlinkAnimationTime);
        }

        public EntitiesDB entitiesDB { get; set; }

        public void Ready() {}

        public GameTickScheduler tickScheduler => GameTickScheduler.Update;
        public int Order => (int)GameEngineOrder.Logic;

        public IEnumerator Tick()
        {
            while (true)
            {
                yield return _conversionWait;
                ProcessNextLevelSelection();
                // Reset conversion wait here so we keep conversion periods constant.
                _conversionWait.Reset();
                yield return ProcessConversion();
            }
        }

        void ProcessNextLevelSelection()
        {
            var shipQuery =
                entitiesDB.QueryEntities<ShipLevelComponent, PointsComponent>(ShipGroups.AliveShipGroups);
            var invalidEgid = new EGID();
            var pirateEgid = invalidEgid;
            var merchantEgid = invalidEgid;
            var pirateCoins = int.MaxValue;
            var merchantCoins = 0;

            foreach (var (shipLevels, points, count) in shipQuery.groups)
            {
                for (var i = 0; i < count; i++)
                {
                    ref var shipLevel = ref shipLevels[i];
                    ref var point = ref points[i];
                    shipLevel.NextLevel = ShipLevel.Normal;

                    if (point.Coins <= pirateCoins)
                    {
                        // Don't allow pirate to be selected when to ships have the same coins.
                        pirateEgid = pirateCoins != point.Coins ? shipLevel.ID : invalidEgid;
                        pirateCoins = point.Coins;
                    }

                    if (point.Coins > merchantCoins)
                    {
                        // Don't allow merchant to be selected when to ships have the same coins.
                        merchantEgid = merchantCoins != point.Coins ? shipLevel.ID : invalidEgid;
                        merchantCoins = point.Coins;
                    }
                }
            }

            if (GroupTagExtensions.Contains<AFLOAT>(pirateEgid.groupID))
            {
                ref var level = ref entitiesDB.QueryEntity<ShipLevelComponent>(pirateEgid);
                level.NextLevel = ShipLevel.Pirate;
            }

            if (GroupTagExtensions.Contains<AFLOAT>(merchantEgid.groupID))
            {
                ref var level = ref entitiesDB.QueryEntity<ShipLevelComponent>(merchantEgid);
                level.NextLevel = ShipLevel.Merchant;
            }
        }

        IEnumerator ProcessConversion()
        {
            _transitionWait.Reset();
            var frame = 0;
            while (_transitionWait.MoveNext())
            {
                AnimateFrame(frame++);
                yield return _animationWait;
            }

            Convert();
        }

        void AnimateFrame(int frame)
        {
            var nextLevelFrame = frame % 2 == 0;
            var shipQuery = entitiesDB.QueryEntities<ShipLevelComponent, ShipViewComponent>(ShipGroups.AllShipGroupsSnapshot);
            foreach (var (shipLevels, shipViews, count) in shipQuery.groups)
            {
                for (var i = 0; i < count; i++)
                {
                    shipViews[i].Renderer.Sprite = nextLevelFrame ? (int)shipLevels[i].NextLevel : (int)shipLevels[i].Level;
                }
            }
        }

        void Convert()
        {
            var shipQuery = entitiesDB.QueryEntities<ShipComponent, ShipLevelComponent, ShipViewComponent>(ShipGroups.AllShipGroupsSnapshot);
            foreach (var (ships, shipLevels, shipViews, count) in shipQuery.groups)
            {
                for (var i = 0; i < count; i++)
                {
                    var changed = shipLevels[i].Level != shipLevels[i].NextLevel;
                    shipLevels[i].Level = shipLevels[i].NextLevel;
                    ships[i].Speed = _speedsByLevel[(int) shipLevels[i].Level];
                    shipViews[i].Renderer.Sprite = (int) shipLevels[i].Level;

                    if (changed)
                    {
                        if (shipLevels[i].Level == ShipLevel.Pirate)
                        {
                            shipViews[i].Audio.PlayOneShot = ShipAudioType.PirateCue;
                        }
                        else if (shipLevels[i].Level == ShipLevel.Merchant)
                        {
                            shipViews[i].Audio.PlayOneShot = ShipAudioType.MerchantCue;
                        }
                    }
                }
            }
        }
    }
}
