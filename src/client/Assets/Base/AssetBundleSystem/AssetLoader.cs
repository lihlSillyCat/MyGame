using UnityEngine;
using System;
using System.Collections;

namespace War.Base
{
    public class AssetLoader : BaseLoader
    {
        private static AssetLoader ms_Instance = null;

        private static bool isReady = false;

        public static bool IsReady
        {
            get
            {
                return isReady;
            }
        }

        [SerializeField]
        protected ThreadPriority m_BackgroundLoadingPriority = ThreadPriority.Normal;

        void Awake()
        {
            isReady = false;
            ms_Instance = this;

            Application.backgroundLoadingPriority = m_BackgroundLoadingPriority;
        }

        void OnDestroy()
        {
            ms_Instance = null;
            isReady = false;
        }

        IEnumerator Start()
        {
            yield return StartCoroutine(Initialize());
            isReady = true;
        }

        public static void LoadAssetAsync(string assetBundleName, string assetName, Action<UnityEngine.Object> callback)
        {
            ms_Instance.StartCoroutine(ms_Instance.Load(assetBundleName, assetName, callback));
        }

        public static void LoadAssetBundleLoadAllAssetsAsync(string assetBundleName, Action<UnityEngine.Object[]> callback)
        {
            ms_Instance.StartCoroutine(ms_Instance.LoadAssetBundleLoadAllAssets(assetBundleName, callback));
        }

        public static void LoadLevelAsync(string assetBundleName, string levelName, bool isAdditive, Action completeCallback)
        {
            ms_Instance.StartCoroutine(LoadLevelProxy(assetBundleName, levelName, isAdditive, completeCallback));
        }

        protected static IEnumerator LoadLevelProxy(string assetBundleName, string levelName, bool isAdditive, Action completeCallback)
        {
            yield return ms_Instance.LoadLevel(assetBundleName, levelName, isAdditive);

            if (completeCallback != null)
            {
                completeCallback();
            }
        }

        public static void UnloadAssetBundle(string assetBundleName, bool unloadAllLoadedObjects = false)
        {
            AssetBundleManager.UnloadAssetBundle(assetBundleName, unloadAllLoadedObjects);
        }
    }
}