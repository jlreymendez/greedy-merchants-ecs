using System;
using Svelto.DataStructures;
using Svelto.ECS;

namespace GreedyMerchants.ECS.Extensions.Svelto
{
    public static class ExclusiveGroupStructExtensions
    {
        static FasterDictionary<uint, FasterDictionary<RefWrapper<Type>, ExclusiveGroupStruct>> _removeTransitions
            = new FasterDictionary<uint, FasterDictionary<RefWrapper<Type>, ExclusiveGroupStruct>>();
        static FasterDictionary<uint, FasterDictionary<RefWrapper<Type>, ExclusiveGroupStruct>> _addTransitions
            = new FasterDictionary<uint, FasterDictionary<RefWrapper<Type>, ExclusiveGroupStruct>>();
        static FasterDictionary<uint, FasterDictionary<RefWrapper<Type>, ExclusiveGroupStruct>> _swapTransitions
            = new FasterDictionary<uint, FasterDictionary<RefWrapper<Type>, ExclusiveGroupStruct>>();

        public static ExclusiveGroupStruct RemoveTag<T>(this ExclusiveGroupStruct group) where T : GroupTag<T>
        {
            if (_removeTransitions.TryGetValue(group, out var transitions))
            {
                var type = new RefWrapper<Type>(typeof(T));
                if (transitions.TryGetValue(type, out var result))
                {
                    return result;
                }
            }

            throw new ECSException("No remove transition found for type "
                .FastConcat(typeof(T).ToString())
                .FastConcat(" in group ").FastConcat(group)
            );
        }

        public static ExclusiveGroupStruct AddTag<T>(this ExclusiveGroupStruct group) where T : GroupTag<T>
        {
            if (_addTransitions.TryGetValue(group, out var transitions))
            {
                var type = new RefWrapper<Type>(typeof(T));
                if (transitions.TryGetValue(type, out var result))
                {
                    return result;
                }
            }

            throw new ECSException("No add transition found for type "
                .FastConcat(typeof(T).ToString())
                .FastConcat(" in group ").FastConcat(group)
            );
        }

        public static ExclusiveGroupStruct SwapTag<TRemoveTag, TAddTag>(this ExclusiveGroupStruct group)
            where TRemoveTag : GroupTag<TRemoveTag> where TAddTag : GroupTag<TAddTag>
        {
            var type =  new RefWrapper<Type>(typeof(GroupTagSwapTemplate<TRemoveTag, TAddTag>));
            if (_swapTransitions.TryGetValue(group, out var transitions))
            {
                if (transitions.TryGetValue(type, out var result))
                {
                    return result;
                }
            }

            throw new ECSException("No swap transition found for types "
                .FastConcat(typeof(TRemoveTag).ToString(), " => ")
                .FastConcat(typeof(TAddTag).ToString())
                .FastConcat(" in group ").FastConcat(group)
            );
        }

        public static void SetTagAddition<T>(this ExclusiveGroupStruct group, ExclusiveGroupStruct target, bool setReverse = true) where T : GroupTag<T>
        {
            if (_addTransitions.TryGetValue(group, out var transitions) == false)
            {
                transitions = new FasterDictionary<RefWrapper<Type>, ExclusiveGroupStruct>();
                _addTransitions[group] = transitions;
            }

            var type = new RefWrapper<Type>(typeof(T));
            transitions[type] = target;

            if (setReverse)
            {
                SetTagRemoval<T>(target, group);
            }
        }

        public static void SetTagRemoval<T>(this ExclusiveGroupStruct group, ExclusiveGroupStruct target, bool setReverse = true) where T : GroupTag<T>
        {
            if (_removeTransitions.TryGetValue(group, out var transitions) == false)
            {
                transitions = new FasterDictionary<RefWrapper<Type>, ExclusiveGroupStruct>();
                _removeTransitions[group] = transitions;
            }

            var type = new RefWrapper<Type>(typeof(T));
            transitions[type] = target;

            if (setReverse)
            {
                SetTagAddition<T>(target, group);
            }
        }


        public static void SetTagSwap<TRemove, TAdd>(this ExclusiveGroupStruct group, ExclusiveGroupStruct target, bool setReverse = true)
            where TRemove : GroupTag<TRemove> where TAdd : GroupTag<TAdd>
        {
            if (_swapTransitions.TryGetValue(group, out var transitions) == false)
            {
                transitions = new FasterDictionary<RefWrapper<Type>, ExclusiveGroupStruct>();
                _swapTransitions[group] = transitions;
            }

            var type = new RefWrapper<Type>(typeof(GroupTagSwapTemplate<TRemove, TAdd>));
            transitions[type] = target;

            if (setReverse)
            {
                SetTagSwap<TAdd, TRemove>(target, group, false);
            }
        }
    }

    static class GroupTagSwapTemplate<TRemove, TAdd> where TRemove : GroupTag<TRemove> where TAdd : GroupTag<TAdd> { }
}