using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ReactNativeGestureHandler.Internal
{
    static class JObjectExtensions
    {
        public static bool ContainsKey(this JObject jsonObject, string key)
        {
            return ((IDictionary<string, JToken>)jsonObject).ContainsKey(key);
        }
    }
}
