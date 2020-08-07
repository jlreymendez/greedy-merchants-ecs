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

        public static ExclusiveGroupStruct SwapTag<TTarget>(this ExclusiveGroupStruct group)
            where TTarget : GroupTag<TTarget>
        {
            var type =  new RefWrapper<Type>(typeof(TTarget));
            if (_swapTransitions.TryGetValue(group, out var transitions))
            {
                if (transitions.TryGetValue(type, out var result))
                {
                    return result;
                }
            }

            throw new ECSException("No swap transition found for type "
                .FastConcat(typeof(TTarget).ToString())
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
                SetTagRemoval<T>(target, group, false);
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
                SetTagAddition<T>(target, group, false);
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

            var type = new RefWrapper<Type>(typeof(TAdd));
            transitions[type] = target;

            // To avoid needing to check if the group already has the tag when swaping (prevent ecs exceptions).
            // The current groups adds the removed tag pointing to itself.
            type = new RefWrapper<Type>(typeof(TRemove));
            transitions[type] = group;

            if (setReverse)
            {
                SetTagSwap<TAdd, TRemove>(target, group, false);
            }
        }
    }
}