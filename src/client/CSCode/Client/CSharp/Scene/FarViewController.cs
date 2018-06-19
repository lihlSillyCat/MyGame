using UnityEngine;
using System.Collections.Generic;

namespace War.Scene
{
    public class FarViewController : MonoBehaviour
    {
        public float xTileSize;
        public float zTileSize;
        public float xSceneSize;
        public float zSceneSize;

        GameObject[] m_FarObjects;
        private int m_xCount;
        private int m_zCount;

        public bool EnableHide;
        public bool EnableShow;
        public bool ForceRefresh;

        int xPos;
        int zPos;


        public Vector3 ShowMax = new Vector3(25, 0, 25);
        public Vector3 ShowMin = new Vector3(1, 0, 1);

        private FarViewManager manager;

        public bool SyncMajorStream = false;

        //[System.NonSerialized]
        //bool[] majorStreamLoaded;

        private void Awake()
        {
            manager = GetComponentInParent<FarViewManager>();
            //if (SyncMajorStream)
            //    majorStreamLoaded = new bool[m_xCount * m_zCount];
        }

        private void Start()
        {
            m_xCount = Mathf.RoundToInt(xSceneSize / xTileSize);
            m_zCount = Mathf.RoundToInt(zSceneSize / zTileSize);
            m_FarObjects = new GameObject[m_xCount * m_zCount];
            

            int count = transform.childCount;
            for(int i = 0; i < count; i++)
            {
                Transform child = transform.GetChild(i);

                float x = child.position.x;
                float z = child.position.z;

                int xIndex = Mathf.RoundToInt(x / xTileSize);
                int zIndex = Mathf.RoundToInt(z / zTileSize);

                int index = GetIndex(xIndex, zIndex);
                m_FarObjects[index] = child.gameObject;
            }
        }

        private void Update()
        {
            if (manager.Player == null)
                return;

            Vector3 pos = manager.Player.position;
            pos -= StreamerManager.GetCurrentMove();

            int xPosCurrent = (xSceneSize != 0) ? (int)(Mathf.FloorToInt(pos.x / xTileSize)) : 0;
            int zPosCurrent = (zSceneSize != 0) ? (int)(Mathf.FloorToInt(pos.z / zTileSize)) : 0;
            if (xPosCurrent != xPos || zPosCurrent != zPos || ForceRefresh )
            {
                xPos = xPosCurrent;
                zPos = zPosCurrent;
                ForceRefresh = false;

                Refresh();
            }
        }

        void Refresh()
        {
            if (!SyncMajorStream)
            {
                for (int z = 0; z < m_zCount; z++)
                {
                    for (int x = 0; x < m_xCount; x++)
                    {
                        int xDiff = Mathf.Abs(x - xPos);
                        int zDiff = Mathf.Abs(z - zPos);

                        GameObject obj = GetFarViewObject(x, z);
                        if (obj)
                        {
                            bool shouldShow = (xDiff < ShowMax.x && zDiff < ShowMax.z) && !(xDiff <= ShowMin.x && zDiff <= ShowMin.z);
                            bool actived = obj.activeSelf;

                            if (actived && !shouldShow && EnableHide)
                                obj.SetActive(false);
                            else if (!actived && shouldShow && EnableShow)
                                obj.SetActive(true);
                        }
                    }
                }
            }
        }

        public void OnStreamerLoaded(int x, int z)
        {
            if (SyncMajorStream)
            {
                GameObject obj = GetFarViewObject(x, z);
                if (obj)
                {
                    bool actived = obj.activeSelf;
                    if (actived)
                    {
                        obj.SetActive(false);
                    }
                }
            }
        }

        public void OnStreamerUnloaded(int x, int z)
        {
            if (SyncMajorStream)
            {
                GameObject obj = GetFarViewObject(x, z);
                if (obj)
                {
                    bool actived = obj.activeSelf;
                    if (!actived)
                    {
                        obj.SetActive(true);
                    }
                }
            }
        }

        GameObject GetFarViewObject(int x, int z)
        {
            if (x >= 0 && x < m_xCount && z >= 0 && z < m_zCount)
                return m_FarObjects[GetIndex(x, z)];
            else
                return null;
        }

        int GetIndex(int x, int z)
        {
            return x + z * m_xCount;
        }



    }   
}