using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using XLua;
#if UNITY_EDITOR
using UnityEditor;
#endif

using War.Base;

namespace War.Script
{
    public class LuaManager : MonoBehaviour
    {
        public const string LuaScriptAssetBundleName = "luascript";
        public const string LuaScriptPath = "Assets/Lua";
        public static LuaManager Instance;

        public LuaEnv luaEnv;

        protected Dictionary<string, byte[]> m_ScriptTextBytes;

        public System.Action<int> BindProgress;

        void Awake()
        {
            Instance = this;
            Object.DontDestroyOnLoad(this);

            m_ScriptTextBytes = new Dictionary<string, byte[]>();
        }

        // Use this for initialization
        IEnumerator Start()
        {
            yield return new WaitUntil(() => AssetLoader.IsReady);
            
            AssetLoader.LoadAssetBundleLoadAllAssetsAsync(LuaScriptAssetBundleName, (assets) =>
            {
                foreach (var asset in assets)
                {
                    var textAsset = asset as TextAsset;
                    m_ScriptTextBytes[textAsset.name] = textAsset.bytes;
                }
            });
            yield return new WaitUntil(() => m_ScriptTextBytes.Count > 0);

            AssetBundleManager.UnloadAssetBundle(LuaScriptAssetBundleName);
            luaEnv = new LuaEnv();
            luaEnv.AddLoader(ScriptLoader);
            luaEnv.DoString("require \"init\"");
        }

        protected byte[] ScriptLoader(ref string fileName)
        {
#if UNITY_EDITOR
            if (AssetBundleManager.SimulateAssetBundleInEditor)
            {
                string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(LuaScriptAssetBundleName, fileName);

                if (assetPaths.Length != 0)
                {
                    fileName = assetPaths[0];

                    TextAsset luaTextAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPaths[0]);
                    return luaTextAsset.bytes;
                }
            }
#endif

            byte[] textBytes;
            m_ScriptTextBytes.TryGetValue(fileName, out textBytes);
            //已经加载过了的
            m_ScriptTextBytes.Remove(fileName);
            return textBytes;
        }

        private void Update()
        {
            if (luaEnv != null)
            {
                luaEnv.Tick();
            }
        }

        void OnDestroy()
        {
#if UNITY_EDITOR
            if (luaEnv != null)
            {
                luaEnv.Dispose();
                luaEnv = null;
            }
#endif

            m_ScriptTextBytes = null;
            Instance = null;
        }
    }
}