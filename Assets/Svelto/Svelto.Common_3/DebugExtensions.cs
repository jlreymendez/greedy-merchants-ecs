namespace Svelto.Common.Internal
{
    public static class DebugExtensions
    {
        public static string TypeName<T>(this T any)
        {
#if DEBUG && !PROFILE_SVELTO          
            var type = any.GetType();
            if (_names.TryGetValue(type, out var name) == false)
            {
                name = type.ToString();
                _names[type] = name;
            }

            return name;
#else
            return "";
#endif
        }
#if DEBUG && !PROFILE_SVELTO          
        static readonly System.Collections.Generic.Dictionary<System.Type, string> _names = new System.Collections.Generic.Dictionary<System.Type, string>();
#endif
    }
}