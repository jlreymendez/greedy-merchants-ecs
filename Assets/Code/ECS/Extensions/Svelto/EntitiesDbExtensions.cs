using Svelto.DataStructures;
using Svelto.ECS;

namespace GreedyMerchants.ECS.Extensions.Svelto
{
    public static class EntitiesDbExtensions
    {
        public static bool Exists<T>(this EntitiesDB db, uint entityId, FasterReadOnlyList<ExclusiveGroupStruct> groups)
            where T : struct, IEntityComponent
        {
            foreach (var group in groups)
            {
                if (db.Exists<T>(entityId, group)) return true;
            }

            return false;
        }
    }
}