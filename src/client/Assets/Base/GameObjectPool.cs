using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;

namespace War.Base
{

    public enum RES_STATE
    {
        RES_STATE_NOT_LOAD, //没有加载
        RES_STATE_LOADING, //加载中
        RES_STATE_LOADED,  //加载完成
    }

    public enum RES_TYPE
    {
        RES_TYPE_DYNAMIC, //动态资源
        RES_TYPE_STATIC, //静态资源
        RES_TYPE_UNKNOWN,//未知资源
        RES_TYPE_MAX,


    }



    public class GameObjectPool :MonoBehaviour
    {
        //加载节点
        public struct Load_Node
        {
            public System.UInt64 key;
            public ObjectPool<Object> pool;

        }
        private static readonly Transform ms_PoolRootTrans;

        private static readonly Dictionary<string, Dictionary<string, ObjectPool<Object>>> ms_Pools
            = new Dictionary<string, Dictionary<string, ObjectPool<Object>>>();

        private static readonly Dictionary<Object, ObjectPool<Object>> ms_Object2Pools
            = new Dictionary<Object, ObjectPool<Object>>();

        //等待加载队列
        private static readonly List<Load_Node>[] m_aryLoadQueue = new List<Load_Node>[(int)RES_TYPE.RES_TYPE_MAX];

        //每次加载的索引
        private static uint ResIndex = 0;

        //最大并发处理数
        private static uint MAX_PROCESS_COUNT = 2;

        //检查的帧间隔
        private static uint MAX_CHECK_FRAME = 1000;

        //是否需要更新
        //private static bool m_bNeedCheckDestroy = false;

        //临时加载节点
        private static Load_Node m_loadNode = new Load_Node();


        //等待回调列表
        private static readonly List<ObjectPool<Object>> m_listWaitCallback = new List<ObjectPool<Object>>();

        //等待释放队列
        private static readonly List<Object> m_listWaitRelease = new List<Object>();

        //检查队列
       // private static readonly List<ObjectPool<Object>> m_listCheck = new List<ObjectPool<Object>>();


       // private static uint m_nLastClearFrame = 0;
        private static uint m_nCurFrame = 0;
       // private static int m_nCheckIndex = 0;

        static GameObjectPool()
        {
            for (int i = 0; i < (int)RES_TYPE.RES_TYPE_MAX; ++i)
            {
                m_aryLoadQueue[i] = new List<Load_Node>();
            }


            GameObject gameObject = new GameObject("GameObjectPool");
            gameObject.AddComponent<GameObjectPool>();
            Object.DontDestroyOnLoad(gameObject);


            var poolObject = new GameObject("Pool");
            Object.DontDestroyOnLoad(poolObject);
           
            ms_PoolRootTrans = poolObject.transform;
            poolObject.SetActive(false);
        }


        //有对象存在，同步返回对象
        public static void GetSync(ref string assetBundleName, ref string assetName, System.Action<Object> callback)
        {
            GetRes(ref assetBundleName, ref assetName, ref callback, (int)RES_TYPE.RES_TYPE_DYNAMIC, 0, true);
        }

        //异步返回对象
        public static void GetAsync(ref string assetBundleName, ref string assetName, System.Action<Object> callback, int resType, int nPriority)
        {

            GetRes(ref assetBundleName, ref assetName, ref callback , resType, nPriority,false);

        }

        public static void GetRes(ref string assetBundleName, ref string assetName, ref System.Action<Object> callback, int resType, int nPriority,bool bSync)
        {

            if (resType < 0 || resType > (int)RES_TYPE.RES_TYPE_MAX)
            {
                resType = (int)RES_TYPE.RES_TYPE_UNKNOWN;
            }


            Dictionary<string, ObjectPool<Object>> assetMaps;
            ms_Pools.TryGetValue(assetBundleName, out assetMaps);
            if (assetMaps == null)
            {
                assetMaps = new Dictionary<string, ObjectPool<Object>>();
                ms_Pools.Add(assetBundleName, assetMaps);
            }

            if (assetName == null)
            {
                assetName = string.Empty;
            }
            ObjectPool<Object> objectPool;
            assetMaps.TryGetValue(assetName, out objectPool);
            if (objectPool == null)
            {
                objectPool = new ObjectPool<Object>(ref assetBundleName, ref assetName,
                                                    ActiveObject, DeactiveObject,
                                                    OnObjectAdd, OnObjectRemove);
                assetMaps.Add(assetName, objectPool);

                //添加到加载队列
                Add2LoadQueue(resType, nPriority, objectPool);

                //m_listCheck.Add(objectPool);

            }
            else
            {
                //已经有了直接加到列表
                if (objectPool.GetWaitCallbackCount() == 0)
                {
                    m_listWaitCallback.Add(objectPool);
                }

            }

            bSync = bSync || (nPriority <= 0);
            if (bSync)
            {
                objectPool.SetSyncLoadFlag(bSync);
            }
            objectPool.SetLastAccessTime(m_nCurFrame);
            objectPool.GetRes(callback);

        }

            //添加到加载队列
            public static void Add2LoadQueue(int resType, int nPriority, ObjectPool<Object> objectPool)
        {
            List<Load_Node> listQueue = m_aryLoadQueue[resType];

            //推动下标增长，保证key不重复
            ++ResIndex;
            System.UInt64 key = ((System.UInt64)nPriority << 32) | ResIndex;

            int nL = 0;
            int nH = listQueue.Count;
            for (nL = 0; nL < nH; nL++)
            {
                m_loadNode = listQueue[nL];
                if (m_loadNode.pool.GetResState() == RES_STATE.RES_STATE_NOT_LOAD)
                {
                    break;
                }

            }



            int nM = (nH + nL) >> 1;
            while (nM < nH && nM > nL)
            {
                m_loadNode = listQueue[nM];
                if (key < m_loadNode.key)
                {
                    nH = nM;
                    nM = (nH + nL) >> 1;
                }
                else
                {
                    nL = nM;
                    nM = (nH + nL) >> 1;
                }
            }



            m_loadNode.key = key;
            m_loadNode.pool = objectPool;
            listQueue.Insert(nM, m_loadNode);
        }

        public static void Release(Object obj)
        {
            //ReleaseImp(obj);
            
            var gameObj = obj as GameObject;
            if (gameObj != null)
            {
                gameObj.transform.SetParent(ms_PoolRootTrans, false);
            }

            m_listWaitRelease.Add(obj);
            
        }

        public static void ReleaseImmediately(Object obj)
        {
            ReleaseImp(obj);
        }

        //释放实现
        private static void ReleaseImp(Object obj)
        {

            //这里判断不出来是真的null还是被destroy了先屏蔽
            if(obj==null)
            {
                return;
            }

            ObjectPool<Object> objectPool;
            ms_Object2Pools.TryGetValue(obj, out objectPool);
            if (objectPool != null)
            {
                //m_bNeedCheckDestroy = true;

                objectPool.Release(obj);
                objectPool.SetLastAccessTime(m_nCurFrame);

                /*

                if (null!= obj)
                {
                    objectPool.Release(obj);
                    objectPool.SetLastAccessTime(m_nCurFrame);
                }
                else
                {
                    Debug.LogError("严重错误内存池对象已经被销毁，请检查：" + objectPool.GetBundleName() + "___" + objectPool.GetAssertName());
                    ms_Object2Poolsms_Object2Poolsms_Object2Pools.Remove(obj);
                }
                */


            }
            else
            {
                Debug.LogErrorFormat("object pool not found, object name: {0}! ", obj.name);
            }

        }

        private static void ActiveObject(Object obj)
        {
            var gameObj = obj as GameObject;
            if (gameObj != null)
            {
                gameObj.transform.SetParent(null, false);
                gameObj.SetActive(true);
            }
        }

        private static void DeactiveObject(Object obj)
        {
            var gameObj = obj as GameObject;
            if (gameObj != null)
            {
                gameObj.transform.SetParent(ms_PoolRootTrans, false);
                gameObj.SetActive(false);
            }
        }

        private static void OnObjectAdd(Object obj, ObjectPool<Object> objectPool)
        {
            ms_Object2Pools.Add(obj, objectPool);
        }

        private static void OnObjectRemove(Object obj, ObjectPool<Object> objectPool)
        {
            ms_Object2Pools.Remove(obj);
        }

        public static void Clear()
        {
            //将所有等待处理的回调清除
            int nCount = m_listWaitRelease.Count;

            for (int i = 0; i < nCount; ++i)
            {
                ReleaseImp(m_listWaitRelease[i]);
            }
            m_listWaitRelease.Clear();


            //处理资源初始化
            ObjectPool<Object> pool = null;
            int nWaitCount = m_listWaitCallback.Count;
            for (int i = 0; i < nWaitCount; ++i)
            {
                pool = m_listWaitCallback[i];
                while (pool.ProcessCallback())
                {

                }
            }





            //bool bHadDestroy = false;
            bool bUnload = false;
            var assetMapToRemove = new List<string>();

            foreach (var assetMapsIter in ms_Pools)
            {
                var assetBundleName = assetMapsIter.Key;
                var assetMaps = assetMapsIter.Value;

                var assetToRemove = new List<string>();
                foreach (var objectPoolIter in assetMaps)
                {
                    var objectPool = objectPoolIter.Value;
                    bUnload = (m_nCurFrame - objectPool.GetLastAccessTime()) > MAX_CHECK_FRAME;
                    if (objectPool.Clear(bUnload))
                    {
                        assetToRemove.Add(objectPoolIter.Key);
                        //bHadDestroy = true;
                    }
                }

                foreach (var key in assetToRemove)
                {
                    assetMaps.Remove(key);
                }

                if (assetMaps.Count == 0)
                {
                    assetMapToRemove.Add(assetBundleName);
                }
            }

            foreach (var key in assetMapToRemove)
            {
                ms_Pools.Remove(key);
            }


            //同步 ms_pool
            /*
            m_listCheck.Clear();
            foreach (var assetMapsIter in ms_Pools.Values)
            {

                foreach (var objectPoolIter in assetMapsIter.Values)
                {
                    m_listCheck.Add(objectPoolIter);

                }
            }*/

            //if(bHadDestroy)
            //{
                //Resources.UnloadUnusedAssets();
                //System.GC.Collect();
            //}
          
        }

        public void Update()
        {

            Load_Node loadNode;
            int nProcessCount = 0;
            int ResQueueCount = 0;
            RES_STATE state;
            List<Load_Node> listQueue;
            int nQueueLen = m_aryLoadQueue.Length;
            for (int i = 0; i < nQueueLen; ++i)
            {
                listQueue = m_aryLoadQueue[i];
                ResQueueCount = listQueue.Count;
                if (ResQueueCount == 0)
                {
                    continue;
                }

                for (int j = 0; j < ResQueueCount && MAX_PROCESS_COUNT > nProcessCount; ++j)
                {
                    loadNode = listQueue[j];
                    state = loadNode.pool.GetResState();

                    switch(state)
                    {
                        case RES_STATE.RES_STATE_NOT_LOAD:
                        {
                                ++nProcessCount;
                                loadNode.pool.LoadRes();
                            }
                            break;
                        case RES_STATE.RES_STATE_LOADED:
                          {

                                listQueue.RemoveAt(j);
                                //加入到等待回调列表
                                m_listWaitCallback.Add(loadNode.pool);
                     
                                nProcessCount = (int)MAX_PROCESS_COUNT;
                            }
                          break;

                        default:
                            {
                                if (m_nCurFrame - loadNode.pool.GetLastAccessTime() > 200)
                                {
                                    loadNode.pool.SetSyncLoadFlag(true);
                                    listQueue.RemoveAt(j);
                                    nProcessCount = (int)MAX_PROCESS_COUNT;
                                }

                            }
                            break;


                    }

                  
                }

                if (MAX_PROCESS_COUNT < nProcessCount)
                {
                    return;
                }



            }

            if (m_listWaitRelease.Count > 0)
            {
                ReleaseImp(m_listWaitRelease[0]);
                m_listWaitRelease.RemoveAt(0);
                ++nProcessCount;

            }





            //处理资源初始化
            ObjectPool<Object> pool = null;
            int nWaitCount = m_listWaitCallback.Count;
            for (int i = 0; i < nWaitCount && MAX_PROCESS_COUNT > nProcessCount; ++i)
            {
                pool = m_listWaitCallback[i];
                if (pool.ProcessCallback())
                {
                    ++nProcessCount;
                }

                //没有了从列表移除
                if (pool.GetWaitCallbackCount() == 0)
                {
                    m_listWaitCallback.RemoveAt(i);
                    return;
                }
            }





            //释放资源
            ++m_nCurFrame;
            /*
           if (m_nCurFrame - m_nLastClearFrame > MAX_CHECK_FRAME)
           {
               m_nLastClearFrame = m_nCurFrame;

               if(m_bNeedCheckDestroy==true)
               {
                   m_bNeedCheckDestroy = false;
                   Clear();
               }

           }*/


        }

        //输出当前内存池对象
        static public string Dump()
        {
            string strDump = "";
            foreach (var assetMapsIter in ms_Pools)
            {
                var assetBundleName = assetMapsIter.Key;
                var assetMaps = assetMapsIter.Value;

                foreach (var objectPoolIter in assetMaps)
                {
                    var objectPool = objectPoolIter.Value;
                    strDump += objectPool.GetBundleName() + "___" + objectPool.GetAssertName() + "\n";
                }
            }

            return strDump;
        }

        // 返回从池里取但不放回池导致引用计数失效的池
        static public string[] CorruptedObjectPools()
        {
            List<string> pools = new List<string>();

            foreach (var kv in ms_Object2Pools)
            {
                if (kv.Key == null)
                {
                    var pool = kv.Value;
                    pools.Add(pool.GetBundleName());
                }
            }
            return pools.ToArray();
        }
    }


    public class ObjectPool<T> where T : Object
    {
        private readonly Stack<T> m_Stack = new Stack<T>();
        private readonly UnityAction<T> m_ActionOnGet;
        private readonly UnityAction<T> m_ActionOnRelease;

        private readonly UnityAction<T, ObjectPool<T>> m_ActionOnAdd;
        private readonly UnityAction<T, ObjectPool<T>> m_ActionOnRemove;

        public Object prefab;

        public int countAll { get; private set; }
        public int countActive { get; private set; }
        public int countInactive { get { return m_Stack.Count; } }

        protected string m_AssetBundleName;
        protected string m_AssetName;
        protected List<System.Action<Object>> m_Callbacks;

        //最后访问的时间
        private uint m_nLastAccessTime = 0;


        //是否已经加载
        private RES_STATE m_eLoadState;

        //是否同步加载
        private bool m_bSyncLoad = false;

        public ObjectPool(ref string assetBundleName, ref string assetName, 
            UnityAction<T> actionOnGet, UnityAction<T> actionOnRelease,
            UnityAction<T, ObjectPool<T>> actionOnAdd, UnityAction<T, ObjectPool<T>> actionOnRemove)
        {
            m_AssetBundleName = assetBundleName;
            m_AssetName = assetName;

            m_ActionOnGet = actionOnGet;
            m_ActionOnRelease = actionOnRelease;

            m_ActionOnAdd = actionOnAdd;
            m_ActionOnRemove = actionOnRemove;

            m_Callbacks = new List<System.Action<Object>>();
            m_eLoadState = RES_STATE.RES_STATE_NOT_LOAD;

        }

        //加载资源
        public void LoadRes()
        {
            if(RES_STATE.RES_STATE_NOT_LOAD!= m_eLoadState)
            {
                return;
            }

            m_eLoadState = RES_STATE.RES_STATE_LOADING;
            AssetLoader.LoadAssetAsync(m_AssetBundleName, m_AssetName, OnResLoaded);
        }

        //取得资源状态
        public RES_STATE GetResState()
        {
            return m_eLoadState;
        }

        //处理回调
        public bool ProcessCallback()
        {
            if (prefab == null)
            {
                return false;
            }

            if(m_Callbacks.Count==0)
            {
                return false;
            }

            System.Action<Object> callback = m_Callbacks[0];
            m_Callbacks.RemoveAt(0);
            callback(Get());

            return true;

        }

        //取得待处理个数
        public int GetWaitCallbackCount()
        {
            return m_Callbacks.Count;
        }


        //资源加载完成
        private void OnResLoaded(Object go)
        {
            m_eLoadState = RES_STATE.RES_STATE_LOADED;
            prefab = go;

            if(m_bSyncLoad)
            {
                while(m_Callbacks.Count>0)
                {
                    System.Action<Object> callback = m_Callbacks[0];
                    m_Callbacks.RemoveAt(0);
                    callback(Get());
                }
               

            }

            //AssetLoader.UnloadAssetBundle(m_AssetBundleName);


        }

        public void GetRes(System.Action<Object> callback)
        {
            countActive++;

            if(m_bSyncLoad)
            {

                if (prefab != null)
                {
                    callback(Get());
                    return;
                }

                LoadRes();


            }

            m_Callbacks.Add(callback);



            /*
           
            else
            {
                m_Callbacks.Add(callback);
            }
            */

        }

        protected T Get()
        {
            T element;
            if (m_Stack.Count == 0)
            {
                element = Object.Instantiate(prefab) as T;
                countAll++;
            }
            else
            {
                element = m_Stack.Pop();

                //看看是否已经被销毁
                if(element==null)
                {
                    --countAll;
                    Debug.LogError("GameObject:Get(): The object has been released ,please check ：" + m_AssetBundleName + "___" + m_AssetName);
                    return Get();
                }
            }
            m_ActionOnAdd(element, this);
            m_ActionOnGet(element);
            return element;
        }

        public void Release(T element)
        {
            countActive--;
            m_ActionOnRemove(element, this);
            m_ActionOnRelease(element);

            if (null!= element)
            {
                if (m_Stack.Count > 0 && ReferenceEquals(m_Stack.Peek(), element))
                    Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
                m_Stack.Push(element);
            }
            else
            {
                --countAll;
                Debug.LogError("GameObject:Release(): The object has been released ,please check ：" + m_AssetBundleName + "___" + m_AssetName);
            }


          
        }

       

        public bool Clear(bool bUnload)
        {

            while (m_Stack.Count > 0)
            {
                --countAll;
                var element = m_Stack.Pop();
                m_ActionOnRemove(element, this);
                Object.DestroyImmediate(element);

            }

            if (countActive == 0&& bUnload)
            {
                prefab = null;
                AssetLoader.UnloadAssetBundle(m_AssetBundleName);
                m_eLoadState = RES_STATE.RES_STATE_NOT_LOAD;
                return true;
            }

            return false;
        }

        public string GetAssertName()
        {
            return m_AssetName;
        }
        
        public string GetBundleName()
        {
            return m_AssetBundleName;
        }

        public uint GetLastAccessTime()
        {
            return m_nLastAccessTime;
        }

        public void SetLastAccessTime(uint nTime)
        {
            m_nLastAccessTime = nTime;
        }

        //是否同步加载
        public void SetSyncLoadFlag(bool bSyncLoad)
        {
            
            m_bSyncLoad = m_bSyncLoad||bSyncLoad;
        }
    }

}