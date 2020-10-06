using System.Collections;
using System.Runtime.CompilerServices;
using GreedyMerchants.ECS.Coin;
using GreedyMerchants.ECS.Extensions.Svelto;
using GreedyMerchants.ECS.Grid;
using GreedyMerchants.ECS.Ship;
using Svelto.ECS;
using Svelto.Tasks.Enumerators;
using Unity.Mathematics;

namespace GreedyMerchants.ECS.AI
{
    public class AIShipTargetSelectionEngine : IQueryingEntitiesEngine
    {
        GridUtils _gridUtils;
        WaitForSecondsEnumerator _decisionWait;

        public AIShipTargetSelectionEngine(GridUtils gridUtils)
        {
            _gridUtils = gridUtils;
            _decisionWait = new WaitForSecondsEnumerator(0.5f);
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
                FindAIShipTargets();
                while (_decisionWait.MoveNext())
                {
                    UpdateTargetPositions();
                    yield return null;
                }
                _decisionWait.Reset();
            }
        }

        void FindAIShipTargets()
        {
            var allShips = entitiesDB.QueryEntities<ShipLevelComponent, ShipViewComponent>(GroupTag<AFLOAT>.Groups).groups;
            var allCoins = entitiesDB.QueryEntities<CoinViewComponent>(CoinGroups.SpawnedCoinsGroup);

            var aiShips = entitiesDB
                .QueryEntities<AiTarget, ShipLevelComponent, ShipViewComponent>(GroupCompound<AI_SHIP, AFLOAT>.Groups);
            foreach (var ((targets, shipLevels, shipViews, count), group) in aiShips.groups)
            {
                for (var i = 0; i < count; i++)
                {
                    var oldTarget = targets[i].Locator;
                    targets[i].Locator = EntityLocator.Invalid;
                    CheckShipsForTarget(ref targets[i], shipLevels[i], shipViews[i], allShips);
                    if (targets[i].Locator != EntityLocator.Invalid) continue;
                    CheckCoinsForTarget(ref targets[i], shipViews[i], allCoins);

                    if (oldTarget != targets[i].Locator)
                    {
                        entitiesDB.PublishEntityChange<AiTarget>(targets[i].ID);
                    }
                }
            }
        }

        void CheckShipsForTarget(ref AiTarget target, ShipLevelComponent level, ShipViewComponent view,
            GroupsEnumerable<ShipLevelComponent, ShipViewComponent> allShips)
        {
            // Check ships.
            var minDistance = 10f; // todo: get number from ship definition.
            var closestTarget = new EGID();
            var closestPosition = uint2.zero;
            foreach (var ((shipLevels, shipViews, count), group) in allShips)
            {
                for (var i = 0; i < count; i++)
                {
                    var otherView = shipViews[i];
                    var otherLevel = shipLevels[i];
                    // We only care about ships we can sink so the level must be below ours.
                    if (otherLevel.Level >= level.Level) continue;

                    var distance = math.distance(otherView.Transform.Position, view.Transform.Position);
                    if (distance < minDistance)
                    {
                        closestTarget = otherView.ID;
                        closestPosition = _gridUtils.WorldToCellPosition(otherView.Transform.Position.xy);
                        minDistance = distance;
                    }
                }
            }
            // Check if we have already have a target
            target.Locator = entitiesDB.GetLocator(closestTarget);
            target.Position = closestPosition;
        }

        void CheckCoinsForTarget(ref AiTarget target, ShipViewComponent view, EntityCollection<CoinViewComponent> allCoins)
        {
            // Check ships.
            var minDistance = 15f; // todo: get number from ship definition.
            var closestCoinId = new EGID();
            var closestPosition = uint2.zero;
            foreach (var coinView in allCoins)
            {
                var distance = math.distance(coinView.Transform.Position, view.Transform.Position);
                if (distance < minDistance)
                {
                    closestCoinId = coinView.ID;
                    closestPosition = _gridUtils.WorldToCellPosition(coinView.Transform.Position.xy);
                    minDistance = distance;
                }
            }
            // Better to do it only once since there is a double indirection here.
            target.Locator = entitiesDB.GetLocator(closestCoinId);
            target.Position = closestPosition;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void UpdateTargetPositions()
        {
            var (targets, count) = entitiesDB.QueryEntities<AiTarget>(AiGroups.AiShip);
            var shipViewsQuery = entitiesDB.QueryEntities<ShipViewComponent>(ShipGroups.AliveShipGroups);

            for (var i = 0; i < count; i++)
            {
                ref var target = ref targets[i];
                var found = false;
                if (entitiesDB.FindEGID(target.Locator, out var egid) == false) continue;
                if (GroupTagExtensions.Contains<AFLOAT>(egid.groupID) == false) continue;

                foreach (var ((shipViews, shipCount), group) in shipViewsQuery.groups)
                {
                    for (var j = 0; j < shipCount; j++)
                    {
                        if (shipViews[j].ID == egid)
                        {
                            target.Position = _gridUtils.WorldToCellPosition(shipViews[j].Transform.Position.xy);
                            found = true;
                            break;
                        }
                    }

                    if (found) break;
                }
            }
        }
    }
}