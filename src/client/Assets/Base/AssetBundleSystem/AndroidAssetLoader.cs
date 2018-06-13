using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace War.Base
{
    public class AndroidAssetLoader
    {
#if UNITY_ANDROID
        const string NativeAssetLoaderDLL = "NativeAssetLoader";

        [DllImport(NativeAssetLoaderDLL)]
        public static extern void SetupAssetManager(IntPtr assetManager, string dataPath);

        [DllImport(NativeAssetLoaderDLL)]
        public static extern int GetStreamAssetLength(string filename);

        [DllImport(NativeAssetLoaderDLL)]
        public static extern int GetStreamAssetContent(string filename, byte[] data, int count);
#endif
    }
}