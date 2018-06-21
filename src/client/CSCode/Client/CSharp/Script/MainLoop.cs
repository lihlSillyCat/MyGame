using UnityEngine;
using System.Collections;

namespace War.Script
{
    public class MainLoop : MonoBehaviour
    {
        public static readonly string AssetBundleEncryptKey = Common.AlgorithmUtils.HashUtf8MD5(",lr,pbope#@^#982(@");
        protected ObjectManager m_ObjectManager;

        void Awake()
        {
            m_ObjectManager = new ObjectManager();
            Base.EncryptResourceWorker.Decryptor = Decryptor;
        }

        private byte[] Decryptor(byte[] content, string assetBundleName)
        {
            return Common.Aes.Decryptor(content, AssetBundleEncryptKey, 
                                        Common.AlgorithmUtils.HashUtf8MD5(assetBundleName).Substring(8, 16));
        }

        void FixedUpdate()
        {
            m_ObjectManager.FixedUpdate(Time.fixedDeltaTime);
        }

        void Update()
        {
            m_ObjectManager.Update(Time.deltaTime);
        }

        void LateUpdate()
        {
            m_ObjectManager.LateUpdate();
        }

        void OnDestroy()
        {
            m_ObjectManager.Destroy();
        }
    }
}