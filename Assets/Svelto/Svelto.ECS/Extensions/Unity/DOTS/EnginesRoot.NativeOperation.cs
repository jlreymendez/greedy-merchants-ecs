#if UNITY_BURST
using System;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.DataStructures.Unity;
using Unity.Jobs.LowLevel.Unsafe;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        //todo: I very likely don't need to create one for each native entity factory, the same can be reused
        readonly AtomicNativeBags _addOperationQueue =
            new AtomicNativeBags(Common.Allocator.Persistent, JobsUtility.MaxJobThreadCount + 1);

        readonly AtomicNativeBags _removeOperationQueue =
            new AtomicNativeBags(Common.Allocator.Persistent, JobsUtility.MaxJobThreadCount + 1);

        readonly AtomicNativeBags _swapOperationQueue =
            new AtomicNativeBags(Common.Allocator.Persistent, JobsUtility.MaxJobThreadCount + 1);

        NativeEntityRemove ProvideNativeEntityRemoveQueue<T>(string memberName) where T : IEntityDescriptor, new()
        {
            //todo: remove operation array and store entity descriptor hash in the return value
            //todo I maybe able to provide a  _nativeSwap.SwapEntity<entityDescriptor> 
            _nativeRemoveOperations.Add(
                new NativeOperationRemove(EntityDescriptorTemplate<T>.descriptor.componentsToBuild, TypeCache<T>.type, memberName));

            return new NativeEntityRemove(_removeOperationQueue, _nativeRemoveOperations.count - 1);
        }
        
        NativeEntitySwap ProvideNativeEntitySwapQueue<T>(string memberName) where T : IEntityDescriptor, new()
        {
            //todo: remove operation array and store entity descriptor hash in the return value
            _nativeSwapOperations.Add(
                new NativeOperationSwap(EntityDescriptorTemplate<T>.descriptor.componentsToBuild, TypeCache<T>.type, memberName));

            return new NativeEntitySwap(_swapOperationQueue, _nativeSwapOperations.count - 1);
        }

        NativeEntityFactory ProvideNativeEntityFactoryQueue<T>() where T : IEntityDescriptor, new()
        {
            //todo: remove operation array and store entity descriptor hash in the return value
            _nativeAddOperations.Add(
                new NativeOperationBuild(EntityDescriptorTemplate<T>.descriptor.componentsToBuild, TypeCache<T>.type));

            return new NativeEntityFactory(_addOperationQueue, _nativeAddOperations.count - 1);
        }

        void NativeOperationSubmission(in PlatformProfiler profiler)
        {
            using (profiler.Sample("Native Remove/Swap Operations"))
            {
                for (int i = 0; i < _removeOperationQueue.count; i++)
                {
                    ref var buffer = ref _removeOperationQueue.GetBuffer(i);

                    while (buffer.IsEmpty() == false)
                    {
                        var componentsIndex = buffer.Dequeue<uint>();
                        var entityEGID = buffer.Dequeue<EGID>();
                        CheckRemoveEntityID(entityEGID, _nativeRemoveOperations[componentsIndex].type); 
                        QueueEntitySubmitOperation(new EntitySubmitOperation(
                                                       EntitySubmitOperationType.Remove, entityEGID, entityEGID
                                                     , _nativeRemoveOperations[componentsIndex].components));
                    }
                }

                for (int i = 0; i < _swapOperationQueue.count; i++)
                {
                    ref var buffer = ref _swapOperationQueue.GetBuffer(i);

                    while (buffer.IsEmpty() == false)
                    {
                        var     componentsIndex = buffer.Dequeue<uint>();
                        var entityEGID      = buffer.Dequeue<DoubleEGID>();
                        
                        CheckRemoveEntityID(entityEGID.@from, _nativeSwapOperations[componentsIndex].type, _nativeSwapOperations[componentsIndex].caller );
                        CheckAddEntityID(entityEGID.to, _nativeSwapOperations[componentsIndex].type, _nativeSwapOperations[componentsIndex].caller); 
                        
                        QueueEntitySubmitOperation(new EntitySubmitOperation(
                                                       EntitySubmitOperationType.Swap, entityEGID.@from, entityEGID.to
                                                     , _nativeSwapOperations[componentsIndex].components));
                    }
                }
            }
            
            using (profiler.Sample("Native Add Operations"))
            {
                for (int i = 0; i < _addOperationQueue.count; i++)
                {
                    ref var buffer = ref _addOperationQueue.GetBuffer(i);
                    
                    while (buffer.IsEmpty() == false)
                    {
                        var componentsIndex = buffer.Dequeue<uint>();
                        var egid            = buffer.Dequeue<EGID>();
                        var componentCounts = buffer.Dequeue<uint>();
                        
                        EntityComponentInitializer init =
                            BuildEntity(egid, _nativeAddOperations[componentsIndex].components, _nativeAddOperations[componentsIndex].type);

                        //only called if Init is called on the initialized (there is something to init)
                        while (componentCounts > 0)
                        {
                            componentCounts--;

                            var typeID = buffer.Dequeue<uint>();

                            IFiller entityBuilder = EntityComponentIDMap.GetTypeFromID(typeID);

                            //after the typeID, I expect the serialized component
                            entityBuilder.FillFromByteArray(init, buffer);
                        }
                    }
                }
            }
        }

        void AllocateNativeOperations()
        {
            _nativeRemoveOperations = new FasterList<NativeOperationRemove>();
            _nativeSwapOperations   = new FasterList<NativeOperationSwap>();
            _nativeAddOperations    = new FasterList<NativeOperationBuild>();
        }

        FasterList<NativeOperationRemove> _nativeRemoveOperations;
        FasterList<NativeOperationSwap>   _nativeSwapOperations;
        FasterList<NativeOperationBuild>  _nativeAddOperations;
    }

    readonly struct DoubleEGID
    {
        internal readonly EGID from;
        internal readonly EGID to;

        public DoubleEGID(EGID from1, EGID to1)
        {
            from = from1;
            to   = to1;
        }
    }

    readonly struct NativeOperationBuild
    {
        internal readonly IComponentBuilder[] components;
        internal readonly Type type;

        public NativeOperationBuild(IComponentBuilder[] descriptorComponentsToBuild, Type entityType)
        {
            type = entityType;
            components = descriptorComponentsToBuild;
        }
    }

    readonly struct NativeOperationRemove
    {
        internal readonly IComponentBuilder[] components;
        internal readonly Type type;
        internal readonly string caller;
        
        public NativeOperationRemove(IComponentBuilder[] descriptorComponentsToRemove, Type entityType, string caller)
        {
            this.caller = caller;
            components = descriptorComponentsToRemove;
            type = entityType;
        }
    }

    readonly struct NativeOperationSwap
    {
        internal readonly IComponentBuilder[] components;
        internal readonly Type type;
        internal readonly string caller;

        public NativeOperationSwap(IComponentBuilder[] descriptorComponentsToSwap, Type entityType, string caller)
        {
            this.caller = caller;
            components = descriptorComponentsToSwap;
            type = entityType;
        }
    }
}
#endif