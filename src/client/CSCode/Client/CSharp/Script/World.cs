using UnityEngine;
using XLua;
using War.Scene;
using War.Game;

namespace War.Script
{
    [CSharpCallLua]
    public delegate void SceneLoadingProgressChangedEventHandler(double progress);

    [CSharpCallLua]
    public delegate void SceneLoadedEventHandler();

    [LuaCallCSharp]
    public class World
    {
        private static readonly GameObject ms_PositionHelper = new GameObject("StreamerPositionHelper",
                                                                    typeof(Base.DontDestroyOnLoad), typeof(ObjectToMove));

        public static void RegisterSceneLoadingCallBack(SceneLoadingProgressChangedEventHandler handler)
        {
            StreamerManager streamerManager = StreamerManager.Instance;
            if (streamerManager != null)
            {
                streamerManager.LoadingProgressChanged += new System.Action<double>(handler);
            }
        }

        public static void RegisterSceneLoadedCallBack(SceneLoadedEventHandler handler)
        {
            StreamerManager streamerManager = StreamerManager.Instance;
            if (streamerManager != null)
            {
                streamerManager.SceneLoaded += new System.Action(handler);
            }
            else
            {
                handler.Invoke();
            }
        }

        public static void NotifyPlayer(GameObject go)
        {
            StreamerManager streamerManager = StreamerManager.Instance;
            if (streamerManager != null)
            {
                streamerManager.SetPlayer(go.transform);
            }
            if (FarViewManager.Instance != null)
            {
                FarViewManager.Instance.Player = go.transform;
            }
        }

        public static void NotifyPosition(float x, float y, float z)
        {
            StreamerManager streamerManager = StreamerManager.Instance;
            if (streamerManager != null)
            {
                ms_PositionHelper.transform.position = GetTilePosition(x, y, z);

                if (streamerManager.HasObjectToMove(ms_PositionHelper.transform) == false)
                    streamerManager.AddObjectToMove(ms_PositionHelper.transform);

                streamerManager.SetPlayer(ms_PositionHelper.transform);
            }

            if (FarViewManager.Instance != null)
            {
                FarViewManager.Instance.Player = ms_PositionHelper.transform;
            }
        }

        public static Vector3 GetTilePosition(float x, float y, float z)
        {
            return StreamerManager.GetTilePosition(x, y, z);
        }

        public static Vector3 GetRealPosition(float x, float y, float z)
        {
            return StreamerManager.GetRealPosition(x, y, z);
        }

        public static float GetTerrainHeight(float x, float y, float z)
        {
            float height = -1000;
            Vector3 pos = new Vector3(x, y, z);
            RaycastHit hit;
            if (Physics.Raycast(pos + new Vector3(0, 10, 0), -Vector3.up, out hit, 1000, LayerConfig.ParachuteMask))
            {
                return hit.point.y;
            }
            return height;
        }

        public static void EnableStreamLoadingUnloading(double index, bool enableloading, bool enableunloading)
        {
            if (StreamerManager.Instance)
            {
                Streamer s = StreamerManager.Instance.streamers[(int)index];
                s.EnableLoading = enableloading;
                s.EnableUnLoading = enableunloading;
                s.ForceRefresh = true;
            }
        }

        public static void NotifyTakeOff()
        {
            if (FarViewManager.Instance)
                FarViewManager.Instance.NotifyTakeoff();
        }

        public static void NotifyDeployed()
        {
            if (FarViewManager.Instance)
                FarViewManager.Instance.NotifyDeployed();
        }

        public static void NotifyLanded()
        {
            if (FarViewManager.Instance)
                FarViewManager.Instance.NotifyLanded();
        }

        public static void NotifyLang(int type)
        {
            if (FarViewManager.Instance)
                FarViewManager.Instance.NotifyLang(type);
        }
    }
}