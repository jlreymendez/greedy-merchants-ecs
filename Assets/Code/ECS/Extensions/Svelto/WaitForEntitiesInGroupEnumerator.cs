﻿using System.Collections;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.Experimental;

namespace GreedyMerchants.ECS.Extensions.Svelto
{
    public class WaitForEntitiesInGroupEnumerator<T> : IEnumerator where T : struct, IEntityComponent
    {
        readonly FasterReadOnlyList<ExclusiveGroupStruct> _groups;
        readonly EntitiesDB _entitiesDB;


        public WaitForEntitiesInGroupEnumerator(FasterReadOnlyList<ExclusiveGroupStruct> groups, EntitiesDB entitiesDB)
        {
            _groups = groups;
            _entitiesDB = entitiesDB;
        }

        public bool MoveNext()
        {
            var query = new QueryGroups(_groups).WithAny<T>(_entitiesDB);
            return query.result.count == 0;
        }

        public void Reset() { }

        public void Reset(float seconds) { }

        public object Current => null;
    }
}