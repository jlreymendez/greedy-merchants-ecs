using System;
using System.Runtime.CompilerServices;
using Svelto.Common;

namespace Svelto.DataStructures
{
    public struct
        SveltoDictionary<TKey, TValue, TKeyStrategy, TValueStrategy> : ISveltoDictionary<TKey, TValue>
        where TKey : IEquatable<TKey>
        where TKeyStrategy : struct, IBufferStrategy<FasterDictionaryNode<TKey>>
        where TValueStrategy : struct, IBufferStrategy<TValue>
    {
        public SveltoDictionary(uint size) : this(size, Allocator.Persistent) { }

        public SveltoDictionary(uint size, Allocator nativeAllocator) : this()
        {
            //AllocationStrategy must be passed external for TValue because SveltoDictionary doesn't have struct
            //constraint needed for the NativeVersion
            _valuesInfo = default;
            _valuesInfo.Alloc(size, nativeAllocator);

            _buckets = new NativeStrategy<int>((uint) HashHelpers.GetPrime((int) size), nativeAllocator);

            _values = default;
            _values.Alloc(size, nativeAllocator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IBuffer<TValue> GetValues(out uint count)
        {
            count = _freeValueCellIndex;

            return _values.ToBuffer();
        }

        //I should put it back to int
        public uint count => _freeValueCellIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(TKey key, in TValue value)
        {
            if (AddValue(key, in value, out _) == false)
                throw new SveltoDictionaryException("Key already present");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(TKey key, in TValue value) { AddValue(key, in value, out _); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (_freeValueCellIndex == 0)
                return;

            _freeValueCellIndex = 0;

            _buckets.Clear();
            _values.Clear();
            _valuesInfo.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FastClear()
        {
            if (_freeValueCellIndex == 0)
                return;

            _freeValueCellIndex = 0;

            _buckets.Clear();
            _valuesInfo.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(TKey key) { return TryFindIndex(key, out _); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SveltoDictionaryKeyValueEnumerator GetEnumerator()
        {
            return new SveltoDictionaryKeyValueEnumerator(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(TKey key, out TValue result)
        {
            if (TryFindIndex(key, out var findIndex) == true)
            {
                result = _values[(int) findIndex];
                return true;
            }

            result = default;
            return false;
        }

        //todo: can be optimized
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetOrCreate(TKey key)
        {
            if (TryFindIndex(key, out var findIndex) == true)
            {
                return ref _values[(int) findIndex];
            }

            AddValue(key, default, out findIndex);

            return ref _values[(int) findIndex];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetOrCreate(TKey key, Func<TValue> builder)
        {
            if (TryFindIndex(key, out var findIndex) == true)
            {
                return ref _values[(int) findIndex];
            }

            AddValue(key, builder(), out findIndex);

            return ref _values[(int) findIndex];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetDirectValueByRef(uint index) { return ref _values[index]; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetValueByRef(TKey key)
        {
            if (TryFindIndex(key, out var findIndex) == true)
                return ref _values[(int) findIndex];

            throw new SveltoDictionaryException("Key not found");
        }

        public void SetCapacity(uint size)
        {
            if (_values.capacity < size)
            {
                _values.Resize(size);
                _valuesInfo.Resize(size);
            }
        }

        public TValue this[TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[(int) GetIndex(key)];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => AddValue(key, in value, out _);
        }

        bool AddValue(TKey key, in TValue value, out uint indexSet)
        {
            int  hash        = key.GetHashCode();
            uint bucketIndex = Reduce((uint) hash, (uint) _buckets.capacity);

            if (_freeValueCellIndex == _values.capacity)
            {
                var expandPrime = HashHelpers.ExpandPrime((int) _freeValueCellIndex);

                _values.Resize((uint) expandPrime);
                _valuesInfo.Resize((uint) expandPrime);
            }

            //buckets value -1 means it's empty
            var valueIndex = _buckets[bucketIndex] - 1;

            if (valueIndex == -1)
                //create the info node at the last position and fill it with the relevant information
                _valuesInfo[_freeValueCellIndex] = new FasterDictionaryNode<TKey>(ref key, hash);
            else //collision or already exists
            {
                int currentValueIndex = valueIndex;
                do
                {
                    //must check if the key already exists in the dictionary
                    //for some reason this is faster than using Comparer<TKey>.default, should investigate
                    ref var fasterDictionaryNode = ref _valuesInfo[currentValueIndex];
                    if (fasterDictionaryNode.hashcode == hash && fasterDictionaryNode.key.Equals(key) == true)
                    {
                        //the key already exists, simply replace the value!
                        _values[currentValueIndex] = value;
                        indexSet                   = (uint) currentValueIndex;
                        return false;
                    }

                    currentValueIndex = fasterDictionaryNode.previous;
                } while (currentValueIndex != -1); //-1 means no more values with key with the same hash

                //oops collision!
                _collisions++;
                //create a new node which previous index points to node currently pointed in the bucket
                _valuesInfo[_freeValueCellIndex] = new FasterDictionaryNode<TKey>(ref key, hash, valueIndex);
                //update the next of the existing cell to point to the new one
                //old one -> new one | old one <- next one
                _valuesInfo[valueIndex].next = (int) _freeValueCellIndex;
                //Important: the new node is always the one that will be pointed by the bucket cell
                //so I can assume that the one pointed by the bucket is always the last value added
                //(next = -1)
            }

            //item with this bucketIndex will point to the last value created
            //ToDo: if instead I assume that the original one is the one in the bucket
            //I wouldn't need to update the bucket here. Small optimization but important
            _buckets[bucketIndex]        = (int) (_freeValueCellIndex + 1);
            _values[_freeValueCellIndex] = value;

            indexSet = _freeValueCellIndex;
            _freeValueCellIndex++;

            //too many collisions?asd
            if (_collisions > _buckets.capacity)
            {
                //we need more space and less collisions
                _buckets.Dispose();
                _buckets = new NativeStrategy<int>((uint) HashHelpers.ExpandPrime((int) _collisions)
                                                 , _buckets.allocationStrategy);
                _collisions = 0;

                //we need to get all the hash code of all the values stored so far and spread them over the new bucket
                //length
                for (int newValueIndex = 0; newValueIndex < _freeValueCellIndex; newValueIndex++)
                {
                    //get the original hash code and find the new bucketIndex due to the new length
                    ref var fasterDictionaryNode = ref _valuesInfo[newValueIndex];
                    bucketIndex = Reduce((uint) fasterDictionaryNode.hashcode, (uint) _buckets.capacity);
                    //bucketsIndex can be -1 or a next value. If it's -1 means no collisions. If there is collision,
                    //we create a new node which prev points to the old one. Old one next points to the new one.
                    //the bucket will now points to the new one
                    //In this way we can rebuild the linkedlist.
                    //get the current valueIndex, it's -1 if no collision happens
                    int existingValueIndex = _buckets[bucketIndex] - 1;
                    //update the bucket index to the index of the current item that share the bucketIndex
                    //(last found is always the one in the bucket)
                    _buckets[bucketIndex] = newValueIndex + 1;
                    if (existingValueIndex != -1)
                    {
                        //oops a value was already being pointed by this cell in the new bucket list,
                        //it means there is a collision, problem
                        _collisions++;
                        //the bucket will point to this value, so 
                        //the previous index will be used as previous for the new value.
                        fasterDictionaryNode.previous = existingValueIndex;
                        fasterDictionaryNode.next     = -1;
                        //and update the previous next index to the new one
                        _valuesInfo[existingValueIndex].next = newValueIndex;
                    }
                    else
                    {
                        //ok nothing was indexed, the bucket was empty. We need to update the previous
                        //values of next and previous
                        fasterDictionaryNode.next     = -1;
                        fasterDictionaryNode.previous = -1;
                    }
                }
            }

            return true;
        }

        public bool Remove(TKey key)
        {
            int  hash        = key.GetHashCode();
            uint bucketIndex = Reduce((uint) hash, (uint) _buckets.capacity);

            //find the bucket
            int indexToValueToRemove = _buckets[bucketIndex] - 1;

            //Part one: look for the actual key in the bucket list if found I update the bucket list so that it doesn't
            //point anymore to the cell to remove
            while (indexToValueToRemove != -1)
            {
                ref var fasterDictionaryNode = ref _valuesInfo[indexToValueToRemove];
                if (fasterDictionaryNode.hashcode == hash && fasterDictionaryNode.key.Equals(key) == true)
                {
                    //if the key is found and the bucket points directly to the node to remove
                    if (_buckets[bucketIndex] - 1 == indexToValueToRemove)
                    {
                        DBC.Common.Check.Require(fasterDictionaryNode.next == -1
                                               , "if the bucket points to the cell, next MUST NOT exists");
                        //the bucket will point to the previous cell. if a previous cell exists
                        //its next pointer must be updated!
                        //<--- iteration order  
                        //                      B(ucket points always to the last one)
                        //   ------- ------- -------
                        //   |  1  | |  2  | |  3  | //bucket cannot have next, only previous
                        //   ------- ------- -------
                        //--> insert order
                        _buckets[bucketIndex] = fasterDictionaryNode.previous + 1;
                    }
                    else
                        DBC.Common.Check.Require(fasterDictionaryNode.next != -1
                                               , "if the bucket points to another cell, next MUST exists");

                    UpdateLinkedList(indexToValueToRemove, ref _valuesInfo);

                    break;
                }

                indexToValueToRemove = fasterDictionaryNode.previous;
            }

            if (indexToValueToRemove == -1)
                return false; //not found!

            _freeValueCellIndex--; //one less value to iterate

            //Part two:
            //At this point nodes pointers and buckets are updated, but the _values array
            //still has got the value to delete. Remember the goal of this dictionary is to be able
            //to iterate over the values like an array, so the values array must always be up to date

            //if the cell to remove is the last one in the list, we can perform less operations (no swapping needed)
            //otherwise we want to move the last value cell over the value to remove
            if (indexToValueToRemove != _freeValueCellIndex)
            {
                //we can move the last value of both arrays in place of the one to delete.
                //in order to do so, we need to be sure that the bucket pointer is updated.
                //first we find the index in the bucket list of the pointer that points to the cell
                //to move
                ref var fasterDictionaryNode = ref _valuesInfo[_freeValueCellIndex];
                var     movingBucketIndex    = Reduce((uint) fasterDictionaryNode.hashcode, (uint) _buckets.capacity);

                //if the key is found and the bucket points directly to the node to remove
                //it must now point to the cell where it's going to be moved
                if (_buckets[movingBucketIndex] - 1 == _freeValueCellIndex)
                    _buckets[movingBucketIndex] = (int) (indexToValueToRemove + 1);

                //otherwise it means that there was more than one key with the same hash (collision), so 
                //we need to update the linked list and its pointers
                int next     = fasterDictionaryNode.next;
                int previous = fasterDictionaryNode.previous;

                //they now point to the cell where the last value is moved into
                if (next != -1)
                    _valuesInfo[next].previous = (int) indexToValueToRemove;
                if (previous != -1)
                    _valuesInfo[previous].next = (int) indexToValueToRemove;

                //finally, actually move the values
                _valuesInfo[indexToValueToRemove] = fasterDictionaryNode;
                _values[indexToValueToRemove]     = _values[_freeValueCellIndex];
            }

            return true;
        }

        public void Trim()
        {
            _values.Resize(_freeValueCellIndex);
            _valuesInfo.Resize(_freeValueCellIndex);
        }

        //I store all the index with an offset + 1, so that in the bucket list 0 means actually not existing.
        //When read the offset must be offset by -1 again to be the real one. In this way
        //I avoid to initialize the array to -1

        public bool TryFindIndex(TKey key, out uint findIndex)
        {
            int  hash        = key.GetHashCode();
            uint bucketIndex = Reduce((uint) hash, (uint) _buckets.capacity);

            int valueIndex = _buckets[bucketIndex] - 1;

            //even if we found an existing value we need to be sure it's the one we requested
            while (valueIndex != -1)
            {
                //for some reason this is way faster than using Comparer<TKey>.default, should investigate
                ref var fasterDictionaryNode = ref _valuesInfo[valueIndex];
                if (fasterDictionaryNode.hashcode == hash && fasterDictionaryNode.key.Equals(key) == true)
                {
                    //this is the one
                    findIndex = (uint) valueIndex;
                    return true;
                }

                valueIndex = fasterDictionaryNode.previous;
            }

            findIndex = 0;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetIndex(TKey key)
        {
            if (TryFindIndex(key, out var findIndex))
                return findIndex;

            throw new SveltoDictionaryException("Key not found");
        }

        // public NativeFasterDictionary<TK, TV> ToNative<TK, TV>() where TK : unmanaged, TKey, IEquatable<TK> 
        //                                                          where TV : unmanaged, TValue
        // {
        //     return new NativeFasterDictionary<TK, TV>(_buckets.ToNativeArray(), (uint) _buckets.capacity
        //                                             , _values.ToNativeArray(), _valuesInfo.ToNativeArray()
        //                                             , _freeValueCellIndex, (uint) _values.capacity);
        // }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static uint Reduce(uint x, uint N)
        {
            if (x >= N)
                return x % N;

            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void UpdateLinkedList(int index, ref TKeyStrategy valuesInfo)
        {
            int next     = valuesInfo[index].next;
            int previous = valuesInfo[index].previous;

            if (next != -1)
                valuesInfo[next].previous = previous;
            if (previous != -1)
                valuesInfo[previous].next = next;
        }

        public ref struct SveltoDictionaryKeyValueEnumerator
        {
            public SveltoDictionaryKeyValueEnumerator
                (SveltoDictionary<TKey, TValue, TKeyStrategy, TValueStrategy> dic) : this()
            {
                _dic   = dic;
                _index = -1;
                _count = (int) dic.count;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
#if DEBUG && !PROFILE_SVELTO
                if (_count != _dic.count)
                    throw new SveltoDictionaryException("can't modify a dictionary during its iteration");
#endif
                if (_index < _count - 1)
                {
                    ++_index;
                    return true;
                }

                return false;
            }

            public KeyValuePairFast Current =>
                new KeyValuePairFast(_dic._valuesInfo[_index].key, _dic._values, _index);

            readonly SveltoDictionary<TKey, TValue, TKeyStrategy, TValueStrategy> _dic;
            readonly int                                                          _count;

            int _index;
        }

        /// <summary>
        ///the mechanism to use arrays is fundamental to work 
        /// </summary>
        public readonly ref struct KeyValuePairFast
        {
            readonly TValueStrategy _dicValues;
            readonly TKey            _key;
            readonly int             _index;

            public KeyValuePairFast(TKey keys, TValueStrategy dicValues, int index)
            {
                _dicValues = dicValues;
                _index     = index;
                _key       = keys;
            }

            public TKey Key => _key;

            //todo: I can't use ToFast here, unboxing would be slower than indexing the array in a IBuffer. I have to use the Strategy here
            public ref TValue Value => ref _dicValues[_index];
        }

        public void Dispose()
        {
            _valuesInfo.Dispose();
            _values.Dispose();
            _buckets.Dispose();
        }

        TKeyStrategy            _valuesInfo;
        NativeStrategy<int>     _buckets;
        uint                    _freeValueCellIndex;
        uint                    _collisions;
        internal TValueStrategy _values;
    }

    public class SveltoDictionaryException : Exception
    {
        public SveltoDictionaryException(string keyAlreadyExisting) : base(keyAlreadyExisting) { }
    }
}