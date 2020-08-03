using System.Collections;
using System.Diagnostics;
using GreedyMerchants.Data;
using GreedyMerchants.Data.Ship;
using Svelto.ECS;
using Svelto.Tasks.Enumerators;
using UnityEditor.Experimental.GraphView;
using Debug = UnityEngine.Debug;

namespace GreedyMerchants.ECS.Ship
{
    public class ShipLevelConversionEngine : IQueryingEntitiesEngine
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
            _animationWait = new WaitForSecondsEnumerator(0.15f);
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
                yield return _conversionWait;
                ProcessNextLevelSelection();
                ProcessConversion().Run();
            }
        }

        void ProcessNextLevelSelection()
        {
            var shipQuery =
                entitiesDB.QueryEntities<ShipLevelComponent, PointsComponent>(GameGroups.Ships);
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

            if (pirateEgid.Equals(invalidEgid) == false)
            {
                ref var level = ref entitiesDB.QueryEntity<ShipLevelComponent>(pirateEgid);
                level.NextLevel = ShipLevel.Pirate;
            }

            if (merchantEgid.Equals(invalidEgid) == false)
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
            var shipQuery = entitiesDB.QueryEntities<ShipLevelComponent, ShipViewComponent>(GameGroups.Ships);
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
            var shipQuery = entitiesDB.QueryEntities<ShipComponent, ShipLevelComponent, ShipViewComponent>(GameGroups.Ships);
            foreach (var (ships, shipLevels, shipViews, count) in shipQuery.groups)
            {
                for (var i = 0; i < count; i++)
                {
                    shipLevels[i].Level = shipLevels[i].NextLevel;
                    ships[i].Speed = _speedsByLevel[(int) shipLevels[i].Level];
                    shipViews[i].Renderer.Sprite = (int) shipLevels[i].Level;
                }
            }
        }
    }
}