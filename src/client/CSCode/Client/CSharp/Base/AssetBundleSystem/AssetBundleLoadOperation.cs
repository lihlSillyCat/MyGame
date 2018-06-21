using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace War.Base
{
    public abstract class AssetBundleLoadOperation : IEnumerator
    {
        public object Current
        {
            get
            {
                return null;
            }
        }

        virtual public float progress
        {
            get
            {
                return 0f;
            }
        }

        public bool MoveNext()
        {
            return !IsDone();
        }

        public void Reset()
        {
        }

        abstract public bool Update();

        abstract public bool IsDone();
    }

    public class AssetBundleLoadLevelSimulationOperation : AssetBundleLoadOperation
    {
        public override float progress
        {
            get { return 1f; }
        }

        public AssetBundleLoadLevelSimulationOperation()
        {
        }

        public override bool Update()
        {
            return false;
        }

        public override bool IsDone()
        {
            return true;
        }
    }

    public class AssetBundleLoadLevelOperation : AssetBundleLoadOperation
    {
        protected string m_AssetBundleName;
        protected string m_LevelName;
        protected bool m_IsAdditive;
        protected string m_DownloadingError;
        protected AsyncOperation m_Request;

        public override float progress
        {
            get
            {
                if (m_Request != null)
                {
                    return m_Request.progress;
                }

                return 0f;
            }
        }

        public AssetBundleLoadLevelOperation(string assetbundleName, string levelName, bool isAdditive)
        {
            m_AssetBundleName = assetbundleName;
            m_LevelName = levelName;
            m_IsAdditive = isAdditive;
        }

        public override bool Update()
        {
            if (m_Request != null)
                return false;

            LoadedAssetBundle bundle = AssetBundleManager.GetLoadedAssetBundle(m_AssetBundleName, out m_DownloadingError);
            if (bundle != null)
            {
                if (string.IsNullOrEmpty(m_LevelName))
                {
                    string[] scenePaths = bundle.m_AssetBundle.GetAllScenePaths();
                    m_LevelName = System.IO.Path.GetFileNameWithoutExtension(scenePaths[0]);
                }

                if (m_IsAdditive)
                {
                    m_Request = SceneManager.LoadSceneAsync(m_LevelName, LoadSceneMode.Additive);
                }
                else
                {
                    m_Request = SceneManager.LoadSceneAsync(m_LevelName, LoadSceneMode.Single);
                }
                return false;
            }
            else
                return true;
        }

        public override bool IsDone()
        {
            // Return if meeting downloading error.
            // m_DownloadingError might come from the dependency downloading.
            if (m_Request == null && m_DownloadingError != null)
            {
                Debug.LogError(m_DownloadingError);
                return true;
            }

            return m_Request != null && m_Request.isDone;
        }
    }

    public abstract class AssetBundleLoadAssetOperation : AssetBundleLoadOperation
    {
        public abstract T GetAsset<T>() where T : UnityEngine.Object;
    }

    public class AssetBundleLoadAssetOperationSimulation : AssetBundleLoadAssetOperation
    {
        Object m_SimulatedObject;

        public AssetBundleLoadAssetOperationSimulation(Object simulatedObject)
        {
            m_SimulatedObject = simulatedObject;
        }

        public override T GetAsset<T>()
        {
            return m_SimulatedObject as T;
        }

        public override bool Update()
        {
            return false;
        }

        public override bool IsDone()
        {
            return true;
        }
    }

    public class AssetBundleLoadAssetOperationFull : AssetBundleLoadAssetOperation
    {
        protected string m_AssetBundleName;
        protected string m_AssetName;
        protected string m_DownloadingError;
        protected System.Type m_Type;
        protected AssetBundleRequest m_Request = null;

        public AssetBundleLoadAssetOperationFull(string bundleName, string assetName, System.Type type)
        {
            m_AssetBundleName = bundleName;
            m_AssetName = assetName;
            m_Type = type;
        }

        public override T GetAsset<T>()
        {
            if (m_Request != null && m_Request.isDone)
                return m_Request.asset as T;
            else
                return null;
        }

        // Returns true if more Update calls are required.
        public override bool Update()
        {
            if (m_Request != null)
                return false;

            LoadedAssetBundle bundle = AssetBundleManager.GetLoadedAssetBundle(m_AssetBundleName, out m_DownloadingError);
            if (bundle != null)
            {
                if (string.IsNullOrEmpty(m_AssetName))
                {
                    m_AssetName = bundle.m_AssetBundle.GetAllAssetNames()[0];
                }
                m_Request = bundle.m_AssetBundle.LoadAssetAsync(m_AssetName, m_Type);
                return false;
            }
            else
            {
                return true;
            }
        }

        public override bool IsDone()
        {
            // Return if meeting downloading error.
            // m_DownloadingError might come from the dependency downloading.
            if (m_Request == null && m_DownloadingError != null)
            {
                Debug.LogError(m_DownloadingError);
                return true;
            }

            return m_Request != null && m_Request.isDone;
        }
    }

    public class AssetBundleLoadManifestOperation : AssetBundleLoadAssetOperationFull
    {
        public AssetBundleLoadManifestOperation(string bundleName, string assetName, System.Type type)
            : base(bundleName, assetName, type)
        {
        }

        public override bool Update()
        {
            base.Update();

            if (m_Request != null && m_Request.isDone)
            {
                AssetBundleManager.AssetBundleManifestObject = GetAsset<AssetBundleManifest>();
                return false;
            }
            else
                return true;
        }
    }

    public abstract class AssetBundleLoadAllAssetsOperation : AssetBundleLoadOperation
    {
        public abstract T[] GetAssets<T>() where T : UnityEngine.Object;
    }

    public class AssetBundleLoadAllAssetsOperationSimulation : AssetBundleLoadAllAssetsOperation
    {
        protected Object[] m_SimulatedObjects;

        public AssetBundleLoadAllAssetsOperationSimulation(Object[] simulatedObjects)
        {
            m_SimulatedObjects = simulatedObjects;
        }

        public override T[] GetAssets<T>()
        {
            T[] assets = new T[m_SimulatedObjects.Length];

            for (int i = 0, count = m_SimulatedObjects.Length; i < count; ++i)
            {
                assets[i] = m_SimulatedObjects[i] as T;
            }
            return assets;
        }

        public override bool Update()
        {
            return false;
        }

        public override bool IsDone()
        {
            return true;
        }
    }

    public class AssetBundleLoadAllAssetsOperationFull : AssetBundleLoadAllAssetsOperation
    {
        protected string m_AssetBundleName;
        protected string m_DownloadingError;
        protected AssetBundleRequest m_Request = null;
        protected AssetBundle m_AssetBundle = null;

        public AssetBundleLoadAllAssetsOperationFull(string assetBundleName)
        {
            m_AssetBundleName = assetBundleName;
        }

        public override T[] GetAssets<T>()
        {
            if (m_Request != null && m_Request.isDone)
                return m_AssetBundle.LoadAllAssets<T>();
            else
                return null;
        }

        public override bool Update()
        {
            if (m_Request != null)
                return false;

            LoadedAssetBundle bundle = AssetBundleManager.GetLoadedAssetBundle(m_AssetBundleName, out m_DownloadingError);
            if (bundle != null)
            {
                m_AssetBundle = bundle.m_AssetBundle;
                m_Request = m_AssetBundle.LoadAllAssetsAsync();
                return false;
            }
            else
            {
                return true;
            }
        }

        public override bool IsDone()
        {
            // Return if meeting downloading error.
            // m_DownloadingError might come from the dependency downloading.
            if (m_Request == null && m_DownloadingError != null)
            {
                Debug.LogError(m_DownloadingError);
                return true;
            }

            return m_Request != null && m_Request.isDone;
        }
    }
}