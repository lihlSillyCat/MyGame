using UnityEngine;
using System.Collections.Generic;

namespace War.Scene
{
    public class BillboardController : MonoBehaviour
    {
        public float xTileSize;
        public float zTileSize;
        public float xSceneSize;
        public float zSceneSize;

        GameObject[] m_BillboardObjects;
        private int m_xCount;
        private int m_zCount;        

        int xPos;
        int zPos;

        private FarViewManager manager;

        private int langType;

        private void Awake()
        {
            langType = 0;
            manager = GetComponentInParent<FarViewManager>();
        }

        private void Start()
        {
            m_xCount = Mathf.RoundToInt(xSceneSize / xTileSize);
            m_zCount = Mathf.RoundToInt(zSceneSize / zTileSize);
            m_BillboardObjects = new GameObject[m_xCount * m_zCount];
            

            int count = transform.childCount;
            for(int i = 0; i < count; i++)
            {
                Transform child = transform.GetChild(i);

                float x = child.position.x;
                float z = child.position.z;

                int xIndex = Mathf.RoundToInt(x / xTileSize);
                int zIndex = Mathf.RoundToInt(z / zTileSize);

                int index = GetIndex(xIndex, zIndex);
                m_BillboardObjects[index] = child.gameObject;
            }
        }

        private void Update()
        {
            if (manager.Player == null)
                return;
            if (langType == 0)
                return;

            Vector3 pos = manager.Player.position;
            pos -= StreamerManager.GetCurrentMove();

            int xPosCurrent = (xSceneSize != 0) ? (int)(Mathf.FloorToInt(pos.x / xTileSize)) : 0;
            int zPosCurrent = (zSceneSize != 0) ? (int)(Mathf.FloorToInt(pos.z / zTileSize)) : 0;
            if (xPosCurrent != xPos || zPosCurrent != zPos)
            {
                xPos = xPosCurrent;
                zPos = zPosCurrent;
                for( int z = -1; z < 2; z++)
                {
                    for (int x = -1; x < 2; x++)
                    {
                        GameObject obj = GetBillboardObject(xPos + x, zPos + z);
                        if (obj)
                        {
                            obj.SetActive(true);
                        }
                    }
                }
            }
        }

        public void OnLangChange(int type)
        {
            langType = type;
            if (langType == 0)
            {
                for (int z = 0; z < m_zCount; z++)
                {
                    for (int x = 0; x < m_xCount; x++)
                    {
                        int xDiff = Mathf.Abs(x - xPos);
                        int zDiff = Mathf.Abs(z - zPos);

                        GameObject obj = GetBillboardObject(x, z);
                        if (obj)
                            obj.SetActive(false);
                    }
                }
            }
        }

        public void OnStreamerLoaded(int x, int z)
        {
            if (langType == 0)
                return;

            GameObject obj = GetBillboardObject(x, z);
            if (obj)
            {
                obj.SetActive(true);
            }
        }

        public void OnStreamerUnloaded(int x, int z)
        {
            if (langType == 0)
                return;

            GameObject obj = GetBillboardObject(x, z);
            if (obj)
            {
                obj.SetActive(false);
            }
        }

        GameObject GetBillboardObject(int x, int z)
        {
            if (x >= 0 && x < m_xCount && z >= 0 && z < m_zCount)
                return m_BillboardObjects[GetIndex(x, z)];
            else
                return null;
        }

        int GetIndex(int x, int z)
        {
            return x + z * m_xCount;
        }



    }   
}