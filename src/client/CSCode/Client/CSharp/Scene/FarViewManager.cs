using UnityEngine;

namespace War.Scene
{
    public class FarViewManager : MonoBehaviour
    {
        public static FarViewManager Instance;

        public FarViewController TerrainController;
        public FarViewController HouseController;
        public BillboardController BillboardCtrller;
        public Transform Player;


        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public void NotifyTakeoff()
        {
            if (TerrainController)
            {
                TerrainController.EnableShow = true;
                TerrainController.EnableHide = false;
                TerrainController.ForceRefresh = true;
            }

            if (HouseController)
            {
                HouseController.EnableShow = true;
                HouseController.EnableHide = false;
                HouseController.ShowMax = new Vector3(23, 0, 23);
                HouseController.ShowMin = Vector3.zero;
                HouseController.ForceRefresh = true;
            }
        }

        public void NotifyDeployed()
        {
            if (TerrainController)
            {
                TerrainController.EnableShow = true;
                TerrainController.EnableHide = true;
                TerrainController.ForceRefresh = true;
            }

            if (HouseController)
            {
                HouseController.EnableShow = true;
                HouseController.EnableHide = true;
                HouseController.ForceRefresh = true;
                HouseController.ShowMax = new Vector3(23, 0, 23);
                HouseController.ShowMin = new Vector3(1, 0, 1);
                HouseController.ForceRefresh = true;
            }
        }

        public void NotifyLanded()
        {
            if (TerrainController)
            {
                TerrainController.EnableShow = true;
                TerrainController.EnableHide = true;
                TerrainController.ForceRefresh = true;
            }

            if (HouseController)
            {
                HouseController.EnableShow = true;
                HouseController.EnableHide = true;
                HouseController.ForceRefresh = true;
                HouseController.ShowMax = new Vector3(5, 0, 5);
                HouseController.ShowMin = new Vector3(1, 0, 1);
                HouseController.ForceRefresh = true;
            }
        }

        public void NotifyLang(int type)
        {
            if (BillboardCtrller)
            {
                BillboardCtrller.OnLangChange(type);
            }
        }

        public void NotifyMajorSteamerLoaded(int x, int z)
        {
            if (TerrainController)
            {
                TerrainController.OnStreamerLoaded(x, z);
            }

            if (BillboardCtrller)
            {
                BillboardCtrller.OnStreamerLoaded(x, z);
            }
        }

        public void NotifyMajorStreamerUnloaded(int x, int z)
        {
            if (TerrainController)
            {
                TerrainController.OnStreamerUnloaded(x, z);
            }

            if (BillboardCtrller)
            {
                BillboardCtrller.OnStreamerUnloaded(x, z);
            }
        }
    }
}
