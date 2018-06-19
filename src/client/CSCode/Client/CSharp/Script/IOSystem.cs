using UnityEngine;
using XLua;
using War.Base;

namespace War.Script
{
    [LuaCallCSharp]
    public class IOSystem
    {
        public static void LoadAssetAsync(string assetBundleName, string assetName, LuaFunction callback)
        {
            AssetLoader.LoadAssetAsync(assetBundleName, assetName, (go) =>
            {
                callback.Call(go);
                callback.Dispose();
            });
        }

        //nBoundIndex 一般对应的是地图索引,不起作用填-1
        public static void LoadLevelAsync(string assetBundleName, string levelName, bool isAdditive, LuaFunction callback)
        {
            AssetLoader.LoadLevelAsync(assetBundleName, levelName, isAdditive, () =>
            {
                callback.Call();
                callback.Dispose();
            });
        }

        public static void LoadAssetBundleAllObjects(string assetBundleName, LuaFunction callback)
        {
            AssetLoader.LoadAssetBundleLoadAllAssetsAsync(assetBundleName, (assets) =>
            {
                callback.Call(assets);
                callback.Dispose();
            });
        }

        public static void UnloadAssetBundle(string assetBundleName, bool unloadAllLoadedObjects = false)
        {
            AssetLoader.UnloadAssetBundle(assetBundleName, unloadAllLoadedObjects);
        }
    }
}