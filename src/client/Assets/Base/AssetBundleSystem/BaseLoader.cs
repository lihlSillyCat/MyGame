using UnityEngine;
using System;
using System.Collections;
#if UNITY_EDITOR	
using UnityEditor;
#endif

namespace War.Base
{
    public class BaseLoader : MonoBehaviour
    {
        const string kAssetBundlesPath = "/AssetBundles/";

        public float loadTimeThreshold = 0.01f;

        // Use this for initialization.
        IEnumerator Start()
        {
            yield return StartCoroutine(Initialize());
        }

        // Initialize the downloading url and AssetBundleManifest object.
        protected IEnumerator Initialize()
        {
            // Don't destroy the game object as we base on it to run the loading script.
            DontDestroyOnLoad(gameObject);

#if UNITY_EDITOR
            Debug.Log("We are " + (AssetBundleManager.SimulateAssetBundleInEditor ? "in Editor simulation mode" : "in normal mode"));
#endif

            string platformFolderForAssetBundles = GetPlatformFolderForAssetBundles();
            AssetBundleManager.BaseDownloadingURL = GetBasePersistentUrl();
            AssetBundleManager.BaseInternalPath = GetStreamingAssetsPath();
            AssetBundleManager.loadTimeThreshold = loadTimeThreshold;

            // Initialize AssetBundleManifest which loads the AssetBundleManifest object.
            var request = AssetBundleManager.Initialize(platformFolderForAssetBundles);
            if (request != null)
                yield return StartCoroutine(request);
        }

        public static string GetBasePersistentUrl()
        {
            string platformFolderForAssetBundles = GetPlatformFolderForAssetBundles();
            string relativePath = GetPersistentRelativeUrl();

            return relativePath + kAssetBundlesPath + platformFolderForAssetBundles + "/";
        }

        public static string GetStreamingAssetsPath()
        {
            string platformFolderForAssetBundles = GetPlatformFolderForAssetBundles();
            string relativePath = GetStreamAssetRelativePath();

            return relativePath + kAssetBundlesPath + platformFolderForAssetBundles + "/";
        }

        public static string GetPersistentRelativeUrl()
        {
            if (Application.isEditor)
                return "file://" + System.Environment.CurrentDirectory.Replace("\\", "/"); // Use the build output folder directly.
            else if (Application.isWebPlayer)
                return System.IO.Path.GetDirectoryName(Application.absoluteURL).Replace("\\", "/") + "/PersistentAssets";
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
                return "file://" + Application.persistentDataPath;
            else if (Application.isMobilePlatform || Application.isConsolePlatform)
                return "file://" + Application.persistentDataPath;
            else if (Application.platform == RuntimePlatform.WindowsPlayer)
                return "file://" + Application.dataPath + "/PersistentAssets";
            else // For standalone player.
                return "file://" + Application.persistentDataPath;
        }

        public static string GetStreamAssetRelativePath()
        {
            if (Application.isEditor)
                return Application.streamingAssetsPath.Replace("\\", "/");
            else if (Application.isWebPlayer)
                return System.IO.Path.GetDirectoryName(Application.absoluteURL).Replace("\\", "/") + "/StreamingAssets";
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
                return Application.streamingAssetsPath;
            else if (Application.isMobilePlatform || Application.isConsolePlatform)
                return Application.streamingAssetsPath;
            else // For standalone player.
                return Application.streamingAssetsPath;
        }

        public static string GetPlatformFolderForAssetBundles()
        {
#if UNITY_EDITOR
            return GetPlatformFolderForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
#else
		return GetPlatformFolderForAssetBundles(Application.platform);
#endif
        }

#if UNITY_EDITOR
        public static string GetPlatformFolderForAssetBundles(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.Android:
                    return "Android";
                case BuildTarget.iOS:
                    return "iOS";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "Windows";
                case BuildTarget.StandaloneOSXIntel:
                case BuildTarget.StandaloneOSXIntel64:
                case BuildTarget.StandaloneOSXUniversal:
                    return "OSX";
                // Add more build targets for your own.
                // If you add more targets, don't forget to add the same platforms to GetPlatformFolderForAssetBundles(RuntimePlatform) function.
                default:
                    return null;
            }
        }
#endif

        static string GetPlatformFolderForAssetBundles(RuntimePlatform platform)
        {
            switch (platform)
            {
                case RuntimePlatform.Android:
                    return "Android";
                case RuntimePlatform.IPhonePlayer:
                    return "iOS";
                case RuntimePlatform.WindowsPlayer:
                    return "Windows";
                case RuntimePlatform.OSXPlayer:
                    return "OSX";
                // Add more build platform for your own.
                // If you add more platforms, don't forget to add the same targets to GetPlatformFolderForAssetBundles(BuildTarget) function.
                default:
#if UNITY_ANDROID
                    return "Android";
#elif UNITY_IOS
                    return "iOS";
#elif UNITY_STANDALONE_OSX
                    return "OSX";
#elif UNITY_STANDALONE
                    return "Windows";
#else
                    return null;
#endif
            }
        }

        protected IEnumerator Load(string assetBundleName, string assetName, Action<UnityEngine.Object> callback)
        {
#if ASSET_LOG
            Debug.Log(string.Format("Start to load [{0}]:[{1}] at frame {2}" ,assetBundleName, assetName, Time.frameCount));
#endif

            // Load asset from assetBundle.
            AssetBundleLoadAssetOperation request = AssetBundleManager.LoadAssetAsync(assetBundleName, assetName, typeof(UnityEngine.Object));
            if (request == null)
                yield break;
            yield return StartCoroutine(request);

            // Get the asset.
            UnityEngine.Object prefab = request.GetAsset<UnityEngine.Object>();
#if ASSET_LOG
            Debug.Log((string.IsNullOrEmpty(assetName) ? assetBundleName : assetName) + (prefab == null ? " isn't" : " is") + " loaded successfully at frame " + Time.frameCount);
#endif

            if (prefab != null && callback != null)
            {
                callback(prefab);
            }
        }

        protected IEnumerator LoadAssetBundleLoadAllAssets(string assetBundleName, Action<UnityEngine.Object[]> callback)
        {
#if ASSET_LOG
            Debug.Log("Start to load " + assetBundleName + " at frame " + Time.frameCount);
#endif

            AssetBundleLoadAllAssetsOperation request = AssetBundleManager.LoadAssetBundleLoadAllAssetsAsync(assetBundleName);
            if (request == null)
                yield break;
            yield return StartCoroutine(request);

            if (callback != null)
            {
                callback(request.GetAssets<UnityEngine.Object>());
            }
        }

        protected IEnumerator LoadLevel(string assetBundleName, string levelName, bool isAdditive)
        {
#if ASSET_LOG
            Debug.Log("Start to load scene " + levelName + " at frame " + Time.frameCount);
#endif

            // Load level from assetBundle.
            AssetBundleLoadOperation request = AssetBundleManager.LoadLevelAsync(assetBundleName, levelName, isAdditive);
            if (request == null)
                yield break;
            yield return StartCoroutine(request);

#if ASSET_LOG
            // This log will only be output when loading level additively.
            Debug.Log("Finish loading scene " + levelName + " at frame " + Time.frameCount);
#endif
        }
    }
}