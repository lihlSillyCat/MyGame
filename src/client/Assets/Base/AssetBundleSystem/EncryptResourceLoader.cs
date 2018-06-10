using UnityEngine;

namespace War.Base
{
    public class EncryptResourceLoader
    {
        public bool isDone;
        public string error;
        public byte[] bytes;

        public string assetBundleName;
        public string assetBundlePath;

        public EncryptResourceLoader(string _assetBundleName, string _assetBundlePath)
        {
            isDone = false;
            error = null;
            bytes = null;
            assetBundleName = _assetBundleName;
            assetBundlePath = _assetBundlePath;
        }

        public void Dispose()
        {
            isDone = false;
            error = null;
            bytes = null;
            assetBundlePath = null;
        }
    }
}