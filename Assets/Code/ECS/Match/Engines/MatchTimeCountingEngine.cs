using System;
using System.Collections;
using System.Diagnostics;
using GreedyMerchants.ECS.Extensions.Svelto;
using GreedyMerchants.ECS.Unity;
using Svelto.DataStructures;
using Svelto.ECS;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GreedyMerchants.ECS.Match.Engines
{
    public class MatchTimeCountingEngine : IQueryingEntitiesEngine
    {
        IEntityFactory _factory;
        GameObjectFactory _gameObjectFactory;
        AssetReference _timerHudReference;
        ITime _time;
        float _initialTimer;

        public MatchTimeCountingEngine(IEntityFactory factory, GameObjectFactory gameObjectFactory, AssetReference timerHudReference, ITime time, float initialTimer)
        {
            _factory = factory;
            _gameObjectFactory = gameObjectFactory;
            _timerHudReference = timerHudReference;
            _time = time;
            _initialTimer = initialTimer;
        }

        public EntitiesDB entitiesDB { get; set; }
        public async void Ready()
        {
            if (_initialTimer <= 0) return;

            if (string.IsNullOrEmpty(_timerHudReference.AssetGUID) == false)
            {
                await _timerHudReference.LoadAssetAsync<GameObject>().Task;
            }

            Tick().Run();
        }

        IEnumerator Tick()
        {
            // Create entity.
            var (go, implementors) = _gameObjectFactory.BuildForEntity(_timerHudReference.Asset as GameObject);
            var initializer = _factory.BuildEntity<MatchDescriptor>(0, MatchGroups.Match, implementors);
            initializer.Init(new TimerComponent{ TimeLeft = _initialTimer });

            yield return null;

            while (true)
            {
                if (Process()) break;

                yield return null;
            }

            _time.TimeScale = 0;
        }

        bool Process()
        {
            ref var timer = ref entitiesDB.QueryUniqueEntity<TimerComponent>(MatchGroups.Match);
            timer.TimeLeft = math.max(0, timer.TimeLeft - _time.DeltaTime);
            ref var view = ref entitiesDB.QueryUniqueEntity<MatchViewComponent>(MatchGroups.Match);
            view.TimerHud.Time = timer.TimeLeft;

            return timer.TimeLeft == 0;
        }
    }
}