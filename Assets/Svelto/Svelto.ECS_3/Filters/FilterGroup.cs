﻿using System;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.DataStructures;
using UnityEngine;

namespace Svelto.ECS
{
    /// <summary>
    ///     In order to complete this feature, I need to be able to detect if the entity pointed by the filter
    ///     is still present.
    ///     This feature should work only with groups where entityID cannot be chosen by the user, so that
    ///     a real sparse set can be used like explained at: https://skypjack.github.io/2020-08-02-ecs-baf-part-9/
    ///     For a sparse set to work, the index in the sparse list must coincide with the ID of the entity
    ///     so that from the dense list (that holds unordered entity index), I can get to the sparse list index
    ///     sparse[0] = position in the dense list of the entity 0
    ///     dense[index] = entity ID but also index in the sparse list of the same entity ID
    /// </summary>
    public struct FilterGroup
    {
        internal FilterGroup(ExclusiveGroupStruct exclusiveGroupStruct)
        {
            _denseListOfIndicesToEntityComponentArray =
                new NativeDynamicArrayCast<uint>(NativeDynamicArray.Alloc<uint>(Allocator.Persistent));
            _reverseEGIDs = new NativeDynamicArrayCast<uint>(NativeDynamicArray.Alloc<uint>(Allocator.Persistent));

            _EIDs                 = new SharedSveltoDictionaryNative<uint, uint>(0, Allocator.Persistent);
            _exclusiveGroupStruct = exclusiveGroupStruct;
        }

        /// <summary>
        /// Todo: how to detect if the indices are still pointing to valid entities
        /// </summary>
        public FilteredIndices filteredIndices => new FilteredIndices(_denseListOfIndicesToEntityComponentArray);

        public void Add<N>(uint entityID, N mapper)  where N:IEGIDMapper
        {
#if DEBUG && !PROFILE_SVELTO
            if (_denseListOfIndicesToEntityComponentArray.isValid == false)
                throw new ECSException($"using an invalid filter");
            if (_EIDs.ContainsKey(entityID) == true)
                throw new ECSException(
                    $"trying to add an existing entity {entityID} to filter {mapper.entityType} with group {mapper.groupID}");
#endif
            //Get the index of the Entity in the component array
            var indexOfEntityInBufferComponent = mapper.GetIndex(entityID);

            //add the index in the list of filtered indices
            _denseListOfIndicesToEntityComponentArray.Add(indexOfEntityInBufferComponent);

            //inverse map: need to get from the index to the entityID. This wouldn't be needed with a real sparseset
            var lastIndex = (uint) (_denseListOfIndicesToEntityComponentArray.Count() - 1);
            _reverseEGIDs.AddAt(lastIndex) = entityID;

            //remember the entities indices. This is needed to remove entities from the filter
            _EIDs.Add(entityID, lastIndex);
        }

        public void Remove(uint entityID)
        {
#if DEBUG && !PROFILE_SVELTO
            if (_denseListOfIndicesToEntityComponentArray.isValid == false)
                throw new ECSException($"invalid Filter");
            if (_EIDs.ContainsKey(entityID) == false)
                throw new ECSException(
                    $"trying to remove a not existing entity {new EGID(entityID, _exclusiveGroupStruct)} from filter");
#endif
            InternalRemove(entityID);
        }

        public bool TryRemove(uint entityID)
        {
#if DEBUG && !PROFILE_SVELTO
            if (_denseListOfIndicesToEntityComponentArray.isValid == false)
                throw new ECSException($"invalid Filter");
#endif
            if (_EIDs.ContainsKey(entityID) == false)
                return false;

            InternalRemove(entityID);

            return true;
        }

        /// <summary>
        ///If filters are not in sync with the operations of remove and swap, filters may end up to point to
        ///invalid indices. I need to put in place a way to be able to recognised an invalid filter.
        ///This is currently a disadvantage of the filters. The filters are not updated by the framework
        ///but they must be updated by the user.
        /// </summary>
        public void RebuildIndicesOnStructuralChange<N>(N mapper) where N:IEGIDMapper
        {
#if DEBUG && !PROFILE_SVELTO
            if (_denseListOfIndicesToEntityComponentArray.isValid == false)
                throw new ECSException($"invalid Filter");
#endif
            _denseListOfIndicesToEntityComponentArray.Clear();
            _reverseEGIDs.Clear();

            foreach (var value in _EIDs)
                if (mapper.FindIndex(value.Key, out var indexOfEntityInBufferComponent))
                {
                    _denseListOfIndicesToEntityComponentArray.Add(indexOfEntityInBufferComponent);
                    var lastIndex = (uint) (_denseListOfIndicesToEntityComponentArray.Count() - 1);
                    _reverseEGIDs.AddAt(lastIndex) = value.Key;
                }

            _EIDs.Clear();

            for (uint i = 0; i < _reverseEGIDs.Count(); i++)
                _EIDs[_reverseEGIDs[i]] = i;
        }

        public void Clear()
        {
#if DEBUG && !PROFILE_SVELTO
            if (_denseListOfIndicesToEntityComponentArray.isValid == false)
                throw new ECSException($"invalid Filter");
#endif
            _EIDs.FastClear();
            _reverseEGIDs.Clear();
            _denseListOfIndicesToEntityComponentArray.Clear();
        }

        internal void Dispose()
        {
#if DEBUG && !PROFILE_SVELTO
            if (_denseListOfIndicesToEntityComponentArray.isValid == false)
                throw new ECSException($"invalid Filter");
#endif
            _denseListOfIndicesToEntityComponentArray.Dispose();
            _EIDs.Dispose();
            _reverseEGIDs.Dispose();
        }

        void InternalRemove(uint entityID)
        {
            var count = (uint) _denseListOfIndicesToEntityComponentArray.Count();
            if (count > 0)
            {
                if (count > 1)
                {
                    //get the index in the filter array of the entity to delete
                    var indexFromEGID = _EIDs[entityID];
                    //get the entityID of the last entity in the filter array
                    uint entityToMove = _reverseEGIDs[count - 1];
                    //remove the entity to delete from the tracked Entity
                    _EIDs.Remove(entityID);
                    //the last index of the last entity is updated to the slot of the deleted entity
                    if (entityToMove != entityID)
                    {
                        _EIDs[entityToMove] = indexFromEGID;
                        //the reverseEGID is updated accordingly
                        _reverseEGIDs[indexFromEGID] = entityToMove;
                    }
                    //finally remove the deleted entity from the filters array
                    _denseListOfIndicesToEntityComponentArray.UnorderedRemoveAt(indexFromEGID);
                }
                else
                {
                    _EIDs.FastClear();
                    _reverseEGIDs.Clear();
                    _denseListOfIndicesToEntityComponentArray.Clear();
                }
            }
        }

        NativeDynamicArrayCast<uint>            _denseListOfIndicesToEntityComponentArray;
        NativeDynamicArrayCast<uint>            _reverseEGIDs; //forced to use this because it's not a real sparse set
        SharedSveltoDictionaryNative<uint, uint> _EIDs;

        readonly ExclusiveGroupStruct _exclusiveGroupStruct;
    }
}