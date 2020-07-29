#if UNITY_2019_1_OR_NEWER
using Svelto.DataStructures;
using Unity.Jobs;
using Svelto.Common;

namespace Svelto.ECS.Extensions.Unity
{
    /// <summary>
    /// Note sorted jobs run in serial
    /// </summary>
    /// <typeparam name="Interface"></typeparam>
    /// <typeparam name="SequenceOrder"></typeparam>
    public abstract class SortedJobifedEnginesGroup<Interface, SequenceOrder> : IJobifiedGroupEngine
        where SequenceOrder : struct, ISequenceOrder where Interface : class, IJobifiedEngine
    {
        protected SortedJobifedEnginesGroup(FasterList<Interface> engines)
        {
            _name = "SortedJobifedEnginesGroup - "+this.GetType().Name;
            _instancedSequence = new Sequence<Interface, SequenceOrder>(engines);
        }

        public JobHandle Execute(JobHandle inputHandles)
        {
            var sequenceItems = _instancedSequence.items;
            JobHandle combinedHandles = inputHandles;
            using (var profiler = new PlatformProfiler(_name))
            {
                for (var index = 0; index < sequenceItems.count; index++)
                {
                    var engine = sequenceItems[index];
                    using (profiler.Sample(engine.name)) combinedHandles = JobHandle.CombineDependencies(combinedHandles, engine.Execute(combinedHandles));
                }
            }

            return combinedHandles; 
        }

        public string name => _name;
        
        readonly string _name;
        readonly Sequence<Interface, SequenceOrder> _instancedSequence;
    } 
    
    public abstract class SortedJobifedEnginesGroup<Interface, Parameter, SequenceOrder>: IJobifiedGroupEngine<Parameter>
        where SequenceOrder : struct, ISequenceOrder where Interface : class, IJobifiedEngine<Parameter>
    {
        protected SortedJobifedEnginesGroup(FasterList<Interface> engines)
        {
            _name = "SortedJobifedEnginesGroup - "+this.GetType().Name;
            _instancedSequence = new Sequence<Interface, SequenceOrder>(engines);
        }

        public JobHandle Execute(JobHandle combinedHandles, ref Parameter param)
        {
            var sequenceItems = _instancedSequence.items;
            using (var profiler = new PlatformProfiler(_name))
            {
                for (var index = 0; index < sequenceItems.count; index++)
                {
                    var engine = sequenceItems[index];
                    using (profiler.Sample(engine.name)) combinedHandles =
                        JobHandle.CombineDependencies(combinedHandles, engine.Execute(combinedHandles, ref param));
                }
            }

            return combinedHandles;
        }

        public string name => _name;
        
        readonly string _name;
        readonly Sequence<Interface, SequenceOrder> _instancedSequence;
    }
}
#endif