﻿using System.Collections;
using GreedyMerchants.ECS.Extensions.Svelto;
using GreedyMerchants.ECS.Unity.Extensions;
using Svelto.ECS;
using Svelto.ECS.Experimental;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GreedyMerchants.ECS.Ship
{
    public class ShipHudUpdatingEngine : IQueryingEntitiesEngine
    {
        readonly AssetReference _pointsCanvasReference;
        WaitForEntitiesInGroupEnumerator<ShipComponent> _shipSpawnWait;

        public ShipHudUpdatingEngine(AssetReference pointsCanvasReference)
        {
            _pointsCanvasReference = pointsCanvasReference;
        }

        public EntitiesDB entitiesDB { get; set; }

        public void Ready()
        {
            _shipSpawnWait = new WaitForEntitiesInGroupEnumerator<ShipComponent>(ShipGroups.Ships, entitiesDB);
            Tick().Run();
        }

        public IEnumerator Tick()
        {
            yield return SpawnPointsHud();
            yield return _shipSpawnWait;
            InitialSetup();

            while (true)
            {
                Process();
                yield return null;
            }
        }

        IEnumerator SpawnPointsHud()
        {
            if (string.IsNullOrEmpty(_pointsCanvasReference.AssetGUID)) yield break;
            yield return _pointsCanvasReference.LoadAssetTask<GameObject>();

            if (_pointsCanvasReference.Asset == null) yield break;

            GameObject.Instantiate(_pointsCanvasReference.Asset);
        }

        void InitialSetup()
        {
            var shipQuery = entitiesDB.QueryEntities<ShipComponent, ShipViewComponent>(ShipGroups.Ships);
            foreach (var (ship, shipViews, count) in shipQuery.groups)
            {
                for (var i = 0; i < count; i++)
                {
                    shipViews[i].CoinsHud.Coins = 0;
                    shipViews[i].PointsHud.Color = ship[i].Color;
                    shipViews[i].PointsHud.Points = 0;
                }
            }
        }

        void Process()
        {
            var shipQuery = entitiesDB.QueryEntities<PointsComponent, ShipViewComponent>(ShipGroups.Ships);
            foreach (var (points, shipViews, count) in shipQuery.groups)
            {
                for (var i = 0; i < count; i++)
                {
                    shipViews[i].CoinsHud.Coins = points[i].Coins;
                    shipViews[i].PointsHud.Points = points[i].Points;
                }
            }
        }
    }
}