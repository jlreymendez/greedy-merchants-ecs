using System.Collections;
using GreedyMerchants.ECS.Extensions.Svelto;
using GreedyMerchants.ECS.Unity;
using Svelto.DataStructures;
using Svelto.ECS.Schedulers;
using Svelto.ECS.Schedulers.Unity;
using Svelto.Tasks;

namespace GreedyMerchants.Unity
{
    public class GameRunner
    {
        FasterList<ITickingEngine> _earlyTickingEngines = new FasterList<ITickingEngine>();
        FasterList<ITickingEngine> _tickingEngines = new FasterList<ITickingEngine>();
        FasterList<ITickingEngine> _lateTickingEngines = new FasterList<ITickingEngine>();
        bool _paused;
        bool _playing;
        IEntitiesSubmissionScheduler _scheduler;
        ITime _time;

        public GameRunner()
        {
            _scheduler = new UnityEntitiesSubmissionScheduler();
            _time = new Time();
        }

        public void Pause()
        {
            _paused = true;
            StandardSchedulers.updateScheduler.isPaused = true;
            StandardSchedulers.lateScheduler.isPaused = true;
        }

        public void Resume()
        {
            _paused = false;
        }

        public void Play()
        {
            if (_playing) return;

            Run(_earlyTickingEngines, StandardSchedulers.earlyScheduler);
            Run(_tickingEngines, StandardSchedulers.updateScheduler);
            Run(_lateTickingEngines, StandardSchedulers.lateScheduler);

            _playing = true;
        }

        public void AddTickEngine(ITickingEngine engine)
        {
            switch (engine.tickScheduler)
            {
                case GameTickScheduler.Early: OrderedInsert(_earlyTickingEngines, engine); break;
                case GameTickScheduler.Update: OrderedInsert(_tickingEngines, engine); break;
                case GameTickScheduler.Late: OrderedInsert(_lateTickingEngines, engine); break;
            }
        }

        public IEntitiesSubmissionScheduler SubmissionScheduler => _scheduler;
        public ITime Time => _time;

        void OrderedInsert(FasterList<ITickingEngine> list, ITickingEngine engine)
        {
            uint i = 0;
            for (; i < list.count; i++)
            {
                if (list[i].Order >= engine.Order) break;
            }

            list.AddAt(i, engine);
        }

        void Run(FasterList<ITickingEngine> list, IRunner<IEnumerator> scheduler)
        {
            for (var i = 0; i < list.count; i++)
            {
                list[i].Tick().RunOnScheduler(scheduler);
            }
        }
    }

    public enum GameTickScheduler
    {
        Early,
        Update,
        Late
    }
}