using System;
using System.Collections;
using GreedyMerchants.ECS.Extensions.Svelto;
using GreedyMerchants.ECS.Unity;
using GreedyMerchants.ECS.Unity.Extensions;
using GreedyMerchants.Unity;
using Svelto.ECS;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GreedyMerchants.ECS.Match.Engines
{
    public class MatchTimeCountingEngine : IQueryingEntitiesEngine, ITickingEngine
    {
        IEntityFactory _factory;
        GameObjectFactory _gameObjectFactory;
        AssetReference _timerHudReference;
        GameRunner _runner;
        ITime _time;
        float _initialTimer;

        public MatchTimeCountingEngine(GameRunner runner, IEntityFactory factory, GameObjectFactory gameObjectFactory, AssetReference timerHudReference, float initialTimer)
        {
            _factory = factory;
            _gameObjectFactory = gameObjectFactory;
            _timerHudReference = timerHudReference;
            _runner = runner;
            _time = runner.Time;
            _initialTimer = initialTimer;
        }

        public EntitiesDB entitiesDB { get; set; }
        public void Ready() { }

        public GameTickScheduler tickScheduler => GameTickScheduler.Early;
        public int Order => (int) GameEngineOrder.Logic;

        public IEnumerator Tick()
        {
            if (_initialTimer <= 0) yield break;

            if (string.IsNullOrEmpty(_timerHudReference.AssetGUID) == false)
            {
                _runner.Pause();
                yield return _timerHudReference.LoadAssetTask<GameObject>();

                // Create entity.
                var (go, implementors) = _gameObjectFactory.BuildForEntity(_timerHudReference.Asset as GameObject);
                var initializer = _factory.BuildEntity<MatchDescriptor>(0, MatchGroups.Match, implementors);
                initializer.Init(new TimerComponent{ TimeLeft = _initialTimer });

                yield return null;
                _runner.Resume();
            }

            while (true)
            {
                if (Process()) break;
                yield return null;
            }

            _runner.Pause();
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