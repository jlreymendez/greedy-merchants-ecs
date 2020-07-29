using DBC.ECS;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    /// <summary>
    /// NOTE THESE ENUMERABLES EXIST TO AVOID BOILERPLATE CODE AS THEY SKIP 0 SIZED GROUPS
    /// However if the normal pattern with the double foreach is used, this is not necessary
    /// Note: atm cannot be ref structs because they are returned in a valuetuple
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    public readonly ref struct GroupsEnumerable<T1, T2, T3, T4> where T1 : struct, IEntityComponent
                                                            where T2 : struct, IEntityComponent
                                                            where T3 : struct, IEntityComponent
                                                            where T4 : struct, IEntityComponent
    {
        readonly EntitiesDB                       _db;
        readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;

        public GroupsEnumerable(EntitiesDB db, in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups)
        {
            _db     = db;
            _groups = groups;
        }

        public ref struct GroupsIterator
        {
            public GroupsIterator(EntitiesDB db, in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups) : this()
            {
                _groups     = groups;
                _indexGroup = -1;
                _entitiesDB = db;
            }

            public bool MoveNext()
            {
                //attention, the while is necessary to skip empty groups
                while (++_indexGroup < _groups.count)
                {
                    var entityCollection1 = _entitiesDB.QueryEntities<T1, T2, T3>(_groups[_indexGroup]);
                    if (entityCollection1.count == 0)
                        continue;
                    var entityCollection2 = _entitiesDB.QueryEntities<T4>(_groups[_indexGroup]);
                    if (entityCollection2.count == 0)
                        continue;

                    Check.Assert(entityCollection1.count == entityCollection2.count
                               , "congratulation, you found a bug in Svelto, please report it");

                    EntityCollection<T1, T2, T3> array  = entityCollection1;
                    var array2 = entityCollection2;
                    _buffers = new EntityCollection<T1, T2, T3, T4>(
                        array.Item1, array.Item2, array.Item3, array2);
                    break;
                }

                return _indexGroup < _groups.count;
            }

            public void Reset() { _indexGroup = -1; }

            public EntityCollection<T1, T2, T3, T4> Current => _buffers;

            readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;

            int                              _indexGroup;
            EntityCollection<T1, T2, T3, T4> _buffers;
            readonly EntitiesDB              _entitiesDB;
        }

        public GroupsIterator GetEnumerator() { return new GroupsIterator(_db, _groups); }
    }

    /// <summary>
    /// ToDo source gen could return the implementation of IBuffer directly, but cannot be done manually
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    public readonly ref struct GroupsEnumerable<T1, T2, T3> where T1 : struct, IEntityComponent
                                                        where T2 : struct, IEntityComponent
                                                        where T3 : struct, IEntityComponent
    {
        readonly EntitiesDB                       _db;
        readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;

        public GroupsEnumerable(EntitiesDB db, in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups)
        {
            _db     = db;
            _groups = groups;
        }

        public ref struct GroupsIterator
        {
            public GroupsIterator(EntitiesDB db, in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups) : this()
            {
                _groups     = groups;
                _indexGroup = -1;
                _entitiesDB = db;
            }

            public bool MoveNext()
            {
                //attention, the while is necessary to skip empty groups
                while (++_indexGroup < _groups.count)
                {
                    EntityCollection<T1, T2, T3> entityCollection = _entitiesDB.QueryEntities<T1, T2, T3>(_groups[_indexGroup]);
                    if (entityCollection.count == 0)
                        continue;

                    _buffers = entityCollection;
                    break;
                }

                return _indexGroup < _groups.count;
            }

            public void Reset() { _indexGroup = -1; }

            public EntityCollection<T1, T2, T3> Current => _buffers;

            readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;

            int                                       _indexGroup;
            EntityCollection<T1, T2, T3> _buffers;
            readonly EntitiesDB                       _entitiesDB;
        }

        public GroupsIterator GetEnumerator() { return new GroupsIterator(_db, _groups); }
    }

    public readonly ref struct GroupsEnumerable<T1, T2> where T1 : struct, IEntityComponent where T2 : struct, IEntityComponent
    {
        public GroupsEnumerable(EntitiesDB db, in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups)
        {
            _db     = db;
            _groups = groups;
        }

        public ref struct GroupsIterator
        {
            public GroupsIterator(EntitiesDB db, in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups) : this()
            {
                _db         = db;
                _groups     = groups;
                _indexGroup = -1;
            }

            public bool MoveNext()
            {
                //attention, the while is necessary to skip empty groups
                while (++_indexGroup < _groups.count)
                {
                    var entityCollection = _db.QueryEntities<T1, T2>(_groups[_indexGroup]);
                    if (entityCollection.count == 0)
                        continue;

                    _buffers = entityCollection;
                    break;
                }

                return _indexGroup < _groups.count;
            }

            public void Reset() { _indexGroup = -1; }

            public EntityCollection<T1, T2> Current => _buffers;

            readonly EntitiesDB                       _db;
            readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;

            int                _indexGroup;
            EntityCollection<T1, T2> _buffers;
        }

        public GroupsIterator GetEnumerator() { return new GroupsIterator(_db, _groups); }

        readonly EntitiesDB                       _db;
        readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;
    }

    public readonly ref struct GroupsEnumerable<T1> where T1 : struct, IEntityComponent
    {
        public GroupsEnumerable(EntitiesDB db, in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups)
        {
            _db     = db;
            _groups = groups;
        }

        public ref struct GroupsIterator
        {
            public GroupsIterator(EntitiesDB db, in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups) : this()
            {
                _db         = db;
                _groups     = groups;
                _indexGroup = -1;
            }

            public bool MoveNext()
            {
                //attention, the while is necessary to skip empty groups
                while (++_indexGroup < _groups.count)
                {
                    var entityCollection = _db.QueryEntities<T1>(_groups[_indexGroup]);
                    if (entityCollection.count == 0)
                        continue;

                    _buffer = entityCollection;
                    break;
                }

                return _indexGroup < _groups.count;
            }

            public void Reset() { _indexGroup = -1; }

            public EntityCollection<T1> Current => _buffer;

            readonly EntitiesDB                       _db;
            readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;

            int    _indexGroup;
            EntityCollection<T1> _buffer;
        }

        public GroupsIterator GetEnumerator() { return new GroupsIterator(_db, _groups); }

        readonly EntitiesDB                       _db;
        readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;
    }
}