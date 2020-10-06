﻿using System;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        /// <summary>
        /// todo: EnginesRoot was a weakreference to give the change to inject
        /// entity functions from other engines root. It probably should be reverted
        /// </summary>
        class GenericEntityFunctions : IEntityFunctions
        {
            internal GenericEntityFunctions(EnginesRoot weakReference)
            {
                _enginesRoot = new Svelto.DataStructures.WeakReference<EnginesRoot>(weakReference);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void RemoveEntity<T>(uint entityID, ExclusiveGroupStruct groupID) where T :
                IEntityDescriptor, new()
            {
                RemoveEntity<T>(new EGID(entityID, groupID));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void RemoveEntity<T>(EGID entityEGID) where T : IEntityDescriptor, new()
            {
                DBC.ECS.Check.Require(entityEGID.groupID != 0, "invalid group detected");
                _enginesRoot.Target.CheckRemoveEntityID(entityEGID, TypeCache<T>.type);

                _enginesRoot.Target.QueueEntitySubmitOperation<T>(
                    new EntitySubmitOperation(EntitySubmitOperationType.Remove, entityEGID, entityEGID,
                        EntityDescriptorTemplate<T>.descriptor.componentsToBuild));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void RemoveEntitiesFromGroup(ExclusiveGroupStruct groupID)
            {
                _enginesRoot.Target.RemoveGroupID(groupID);
                DBC.ECS.Check.Require(groupID != 0, "invalid group detected");

                _enginesRoot.Target.QueueEntitySubmitOperation(
                    new EntitySubmitOperation(EntitySubmitOperationType.RemoveGroup, new EGID(0, groupID), new EGID()));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SwapEntitiesInGroup<T>(ExclusiveGroupStruct fromGroupID, ExclusiveGroupStruct toGroupID)
                where T : IEntityDescriptor, new()
            {
#if DEBUG && !PROFILE_SVELTO
                IComponentBuilder[] components = EntityDescriptorTemplate<T>.descriptor.componentsToBuild;

                if (_enginesRoot.Target._groupEntityComponentsDB.TryGetValue(fromGroupID,
                    out var entitiesInGroupPerType) == false)
                    throw new Exception("Entity Serialization failed");

                foreach (var component in components)
                {
                    var dictionary = entitiesInGroupPerType[new RefWrapper<Type>(component.GetEntityComponentType())];

                    dictionary.KeysEvaluator((key) =>
                    {
                        _enginesRoot.Target.CheckRemoveEntityID(new EGID(key, fromGroupID), component.GetType());
                        _enginesRoot.Target.CheckAddEntityID(new EGID(key, fromGroupID), component.GetType());
                    });
                }
#endif

                _enginesRoot.Target.QueueEntitySubmitOperation(
                    new EntitySubmitOperation(EntitySubmitOperationType.SwapGroup, new EGID(0, fromGroupID),
                        new EGID(0, toGroupID)));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SwapEntityGroup<T>(uint entityID, ExclusiveGroupStruct fromGroupID,
                ExclusiveGroupStruct toGroupID)
                where T : IEntityDescriptor, new()
            {
                SwapEntityGroup<T>(new EGID(entityID, fromGroupID), toGroupID);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SwapEntityGroup<T>(EGID fromID, ExclusiveGroupStruct toGroupID)
                where T : IEntityDescriptor, new()
            {
                SwapEntityGroup<T>(fromID, new EGID(fromID.entityID, (uint) toGroupID));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SwapEntityGroup<T>(EGID fromID, ExclusiveGroupStruct toGroupID
              , ExclusiveGroupStruct mustBeFromGroup)
                where T : IEntityDescriptor, new()
            {
                if (fromID.groupID != mustBeFromGroup)
                    throw new ECSException("Entity is not coming from the expected group");

                SwapEntityGroup<T>(fromID, toGroupID);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SwapEntityGroup<T>(EGID fromID, EGID toID
              , ExclusiveGroupStruct mustBeFromGroup)
                where T : IEntityDescriptor, new()
            {
                if (fromID.groupID != mustBeFromGroup)
                    throw new ECSException("Entity is not coming from the expected group");

                SwapEntityGroup<T>(fromID, toID);
            }

#if UNITY_BURST
            public NativeEntityRemove ToNativeRemove<T>(string memberName) where T : IEntityDescriptor, new()
            {
                return _enginesRoot.Target.ProvideNativeEntityRemoveQueue<T>(memberName);
            }

            public NativeEntitySwap ToNativeSwap<T>(string memberName) where T : IEntityDescriptor, new()
            {
                return _enginesRoot.Target.ProvideNativeEntitySwapQueue<T>(memberName);
            }
#endif

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SwapEntityGroup<T>(EGID fromID, EGID toID)
                where T : IEntityDescriptor, new()
            {
                DBC.ECS.Check.Require(fromID.groupID != 0, "invalid group detected");
                DBC.ECS.Check.Require(toID.groupID != 0, "invalid group detected");

                _enginesRoot.Target.CheckRemoveEntityID(fromID, TypeCache<T>.type);
                _enginesRoot.Target.CheckAddEntityID(toID, TypeCache<T>.type);

                _enginesRoot.Target.QueueEntitySubmitOperation<T>(
                    new EntitySubmitOperation(EntitySubmitOperationType.Swap,
                        fromID, toID, EntityDescriptorTemplate<T>.descriptor.componentsToBuild));
            }

            //enginesRoot is a weakreference because GenericEntityStreamConsumerFactory can be injected inside
            //engines of other enginesRoot
            readonly Svelto.DataStructures.WeakReference<EnginesRoot> _enginesRoot;
        }

        void QueueEntitySubmitOperation(EntitySubmitOperation entitySubmitOperation)
        {
#if DEBUG && !PROFILE_SVELTO
            entitySubmitOperation.trace = new System.Diagnostics.StackFrame(1, true);
#endif
            _entitiesOperations.Add((ulong) entitySubmitOperation.fromID, entitySubmitOperation);
        }

        void QueueEntitySubmitOperation<T>(EntitySubmitOperation entitySubmitOperation) where T : IEntityDescriptor
        {
#if DEBUG && !PROFILE_SVELTO
            entitySubmitOperation.trace = new System.Diagnostics.StackFrame(1, true);

            if (_entitiesOperations.TryGetValue((ulong) entitySubmitOperation.fromID, out var entitySubmitedOperation))
            {
                if (entitySubmitedOperation != entitySubmitOperation)
                    throw new ECSException("Only one entity operation per submission is allowed"
                       .FastConcat(" entityComponentType: ")
                       .FastConcat(typeof(T).Name)
                       .FastConcat(" submission type ", entitySubmitOperation.type.ToString(),
                            " from ID: ", entitySubmitOperation.fromID.entityID.ToString())
                       .FastConcat(" previous operation type: ",
                            _entitiesOperations[(ulong) entitySubmitOperation.fromID].type
                               .ToString()));
            }
            else
#endif
                _entitiesOperations.Set((ulong) entitySubmitOperation.fromID, entitySubmitOperation);
        }
    }
}