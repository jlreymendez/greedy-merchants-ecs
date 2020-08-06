using Svelto.DataStructures;
using Svelto.ECS;

namespace GreedyMerchants.ECS.Extensions.Svelto
{
    public class GroupTagExtensions
    {
        public static bool Contains<T>(ExclusiveGroupStruct target) where T : GroupTag<T>
        {
            var groups = GroupTag<T>.Groups;
            foreach (var group in  groups)
            {
                if (group == target) return true;
            }

            return false;
        }
    }
}