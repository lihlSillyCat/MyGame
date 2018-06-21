using UnityEngine;
using XLua;

namespace War.Script
{
    [LuaCallCSharp]
    [ReflectionUse]
    public static class UnityEngineObjectExtention
    {
        public static bool IsNull(this UnityEngine.Object o)
        {
            return o == null;
        }
    }
}