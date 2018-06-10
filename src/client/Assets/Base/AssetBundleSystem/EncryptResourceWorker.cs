using UnityEngine;

using System;
using System.IO;
using System.Threading;
using System.Collections;

namespace War.Base
{
    public class EncryptResourceWorker
    {
        private Queue m_QueueToLoad = new Queue();
        private bool m_Abort = false;

        private int m_StreamingPathLength;
        private string m_StreamingAssetsPath;

        public delegate byte[] DecryptorHandler(byte[] content, string assetBundleName);
        protected static DecryptorHandler ms_Decryptor;

        public static DecryptorHandler Decryptor
        {
            set { ms_Decryptor = value; }
        }

        public EncryptResourceWorker()
        {
            m_StreamingAssetsPath = Application.streamingAssetsPath;
            m_StreamingPathLength = m_StreamingAssetsPath.Length + 1;

#if UNITY_ANDROID && !UNITY_EDITOR
            using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                var _manager = activity.Call<AndroidJavaObject>("getAssets");
                AndroidAssetLoader.SetupAssetManager(_manager.GetRawObject(), Application.dataPath);
            }
#endif

            ThreadPool.QueueUserWorkItem(ResourceLoad);
        }

        public void RequestLoad(EncryptResourceLoader node)
        {
            lock (m_QueueToLoad.SyncRoot)
            {
                m_QueueToLoad.Enqueue(node);
                Monitor.Pulse(m_QueueToLoad.SyncRoot);
            }
        }

        public void ResourceLoad(object state)
        {
            while (!m_Abort)
            {
                EncryptResourceLoader node = null;
                lock (m_QueueToLoad.SyncRoot)
                {
                    while (m_QueueToLoad.Count <= 0 && !m_Abort)
                    {
                        Monitor.Wait(m_QueueToLoad.SyncRoot);
                    }

                    if (m_Abort)
                    {
                        break;
                    }
                    node = (EncryptResourceLoader)m_QueueToLoad.Dequeue();
                }

                if (node != null)
                {
                    byte[] content = null;
                    var assetBundlePath = node.assetBundlePath;
                    try
                    {
#if UNITY_ANDROID && !UNITY_EDITOR
                        if (assetBundlePath.StartsWith(m_StreamingAssetsPath))
                        {
                            var assetPath = assetBundlePath.Substring(m_StreamingPathLength);

                            int assetSize = AndroidAssetLoader.GetStreamAssetLength(assetPath);
                            content = new byte[assetSize];
                            AndroidAssetLoader.GetStreamAssetContent(assetPath, content, assetSize);
                        }
                        else
                        {
                            content = File.ReadAllBytes(assetBundlePath);
                        }
#else
                        content = File.ReadAllBytes(assetBundlePath);
#endif
                    }
                    catch (Exception e)
                    {
                        Debug.LogErrorFormat("Loading asset bundle at path[{0}] error! {1}", assetBundlePath, e.Message);
                    }

                    try
                    {
                        node.bytes = ms_Decryptor.Invoke(content, node.assetBundleName);
                        node.isDone = true;
                    }
                    catch (Exception e)
                    {
                        node.error = e.Message;
                        Debug.LogErrorFormat("decrypt asset bundle {0} error! {1}", node.assetBundleName, e.Message);
                    }
                }
            }
        }

        public void Close()
        {
            m_Abort = true;

            lock (m_QueueToLoad.SyncRoot)
            {
                m_QueueToLoad.Clear();
                Monitor.Pulse(m_QueueToLoad.SyncRoot);
            }
        }
    }
}