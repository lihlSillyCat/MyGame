using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using XLua;
using War.Base;
using System.Net;
using System.Net.Sockets;

namespace War.Script
{
    [LuaCallCSharp]
    public class Net : MonoBehaviour,ISocketSink
    {
        public enum CONNECT_TYPE
        {
            Connect_Type_UDP = 0,
            Connect_Type_TCP = 1,
            Connect_Type_Max = 2
        }

        private const int MAX_ACTION_MODULE_MSG = 400;
        private const int CONNECT_INDEX_NONE = -1;

        private const int MAX_CACHE_SIZE = 100;

        //消息头长度
        private const int MESSAGE_HEAD_LEN = 8;



        //发包对象缓存池
        private ThreadQueue<IPackageData> m_oSendRecycle;

        //收包对象缓存池
        private ThreadQueue<IPackageData> m_oRecieveRecycle;

        //收包队列
        private ThreadQueue<IPackageData> m_oRecieveQueue;

        //接收内存池
        private MemPool m_oRecieveMemPool;

        //当前连接的socketID
        private uint m_nCurSocketID = 0;

        //断线理由
        SOCKET_ERROR m_eReason = SOCKET_ERROR.SOCKET_ERROR_NON;

        // 接收的网络包数量
        public int recvMessageCount
        {
            private set;
            get;
        }

        public bool isConnected
        {
            private set;
            get;
        }

        protected ISocketClient m_CurrentSocketClient;

        private List<ISocketClient> m_SocketList;
   

        protected Queue m_Queue;

        protected enum ConnectErrorCode
        {
            Unknow = -1,
            Success = 0,
            Error = 1,
            Lost = 2
        }
        protected ConnectErrorCode m_ConnErrorCode;

        protected Action<int,int> m_ConnectAction;
        protected LuaFunction m_LuaConnectCallback;

        //当前连接的IP和端口
        private string m_IP;
        private int m_Port;

        protected IMsgHandler m_MessageEventHandler;

        void Awake()
        {
            DontDestroyOnLoad(gameObject);

            isConnected = false;

            m_Queue = new Queue();

            m_oSendRecycle = new ThreadQueue<IPackageData>();
            m_oSendRecycle.Create();
            m_oRecieveRecycle = new ThreadQueue<IPackageData>();
            m_oRecieveRecycle.Create();
            m_oRecieveQueue = new ThreadQueue<IPackageData>();
            m_oRecieveQueue.Create();

            m_oRecieveMemPool = new MemPool();
            m_oRecieveMemPool.Create();


            m_CurrentSocketClient = null;


            m_SocketList = new List<ISocketClient>();


            m_IP = "127.0.0.1";
            m_Port = 0;
  

        }

        public void RegistMessageFunction(string callback)
        {
            m_MessageEventHandler = new LuaMsgHandler();
            m_MessageEventHandler.Create(LuaManager.Instance.luaEnv, callback);
        }

        void Start()
        {
           // StartCoroutine(ProccessMessage());
        }

        private void Update()
        {
            ISocketClient socketClient = null;
            int nCount = m_SocketList.Count;
            for(int i=0;i<nCount;++i)
            {
                socketClient = m_SocketList[i];
                if(socketClient.CanClose())
                {
                    socketClient.Close();
                    m_SocketList.RemoveAt(i);
                    break;
                }

            }

            //推动socket client 更新
            if (null != m_CurrentSocketClient)
            {
                m_CurrentSocketClient.Update();
            }

            //处理解包
            ProccessMessage();
        }


        void ProccessMessage()
        {
           // while (true)
            {
                //yield return new WaitForFixedUpdate();

                //检查是否有连接错误
                CheckConnectError();
                if (m_CurrentSocketClient != null && m_MessageEventHandler != null)
                {
                    recvMessageCount = 0;
                    QueueNode<IPackageData> node = null;
                    do
                    {
                        node = m_oRecieveQueue.Pop();
                        if (null != node)
                        {
                            executePacket(node.item);
                            recvMessageCount++;

                            if (m_oRecieveRecycle.Size() < MAX_CACHE_SIZE)
                            {
                                PackageData packData = (PackageData)node.item;
                                if (null != packData)
                                {
                                    // packData.m_senddata = null;
                                    packData.Release(m_oRecieveMemPool);
                                }

                                m_oRecieveRecycle.Push(node);
                            }
                            else
                            {
                                node.item.Release(m_oRecieveMemPool);
                                node.item = null;

                            }
                        }
                    } while (null != node);
                }
            }
        }

        protected void executePacket(IPackageData data)//UInt16 packetID, byte[] buffer, UInt16 bufferLen)
        {
            if(data.GetClassID()!=m_nCurSocketID)
            {
                Debug.LogWarningFormat("executePacket: Ignore the old connection data");
                return;
            }

            PackageData packdata = (PackageData)data;
            try
            {
                m_MessageEventHandler.OnHandler(packdata.serverID, packdata.msgID, packdata.recivedata.item.data, packdata.recivedata.item.data.Length);
            }
            catch (LuaException e)
            {
                Debug.LogError(e.Message);
            }
        }

        //接收数据
        public void OnReceive(uint nSocketID, byte[] buffer, int nOffset, int size)
        {
            if (null == m_CurrentSocketClient || m_nCurSocketID!= nSocketID)
            {
                return;
            }

            QueueNode<IPackageData> node =m_oRecieveRecycle.Pop();
            if (null == node)
            {
                node = new QueueNode<IPackageData>();
                node.item = new PackageData();
            }

            node.item.Unpack(buffer, nOffset, size, m_oRecieveMemPool);
            node.item.SetClassID(nSocketID);


            //压到接收队列
            m_oRecieveQueue.Push(node);

        }

        //连接成功
        public void OnConnect(uint nSocketID)
        {
            if (null == m_CurrentSocketClient || m_nCurSocketID != nSocketID)
            {
                return;
            }

            isConnected = true;
            m_ConnErrorCode = ConnectErrorCode.Success;
        }

        //关闭
        public void OnClose(uint nSocketID)
        {
            if (null == m_CurrentSocketClient || m_nCurSocketID != nSocketID)
            {
                return;
            }

            // Debug.Log("Connection is close!");
            m_eReason = SOCKET_ERROR.SOCKET_ERROR_CONNECT_CLOSE;
            m_ConnErrorCode = ConnectErrorCode.Lost;
        }

        //连接失败
        public void OnConnectError(uint nSocketID, SOCKET_ERROR eReason )
        {
            if (null == m_CurrentSocketClient || m_nCurSocketID != nSocketID)
            {
                return;
            }

            // Debug.Log("Connect error!");
            m_ConnErrorCode = ConnectErrorCode.Error;
            m_eReason = eReason;

           
        }

        //重新连接
        public void OnReconnect(uint nSocketID)
        {
            if (null == m_CurrentSocketClient || m_nCurSocketID != nSocketID)
            {
                return;
            }

            m_ConnErrorCode = ConnectErrorCode.Lost;
        }

        //发送一个数据包完毕
        public void OnFinishSendData(QueueNode<IPackageData> node)
        {
            if(m_oSendRecycle.Size()< MAX_CACHE_SIZE)
            {
                PackageData packData = (PackageData)node.item;
                if(null!= packData)
                {
                    packData.Release(m_oRecieveMemPool);
                }
                m_oSendRecycle.Push(node);
            }
            else
            {
                node.item.Release(m_oRecieveMemPool);
                node.item = null;

            }

        }

        //连接
        public void Connect(string ip, int port , int type, LuaFunction callback)
        {
            if (true == isConnected)
            {
                //Debug.LogError("已经连接上了");
                return;
            }

            m_IP = ip;
            m_Port = port;

            //不是同一个callback，释放掉
            if (callback != m_LuaConnectCallback || false == callback.Equals(m_LuaConnectCallback))
            {
                if (m_LuaConnectCallback != null)
                {
                    m_LuaConnectCallback.Dispose();
                }
                m_LuaConnectCallback = callback;

                m_ConnectAction = delegate (int errorCode,int reason)
                {
                    m_LuaConnectCallback.Action<int,int>(errorCode, reason);
                };
            }
            Debug.Log(" udp==0,tcp==1 当前尝试连接的类型 :" + type + "连接IP" + m_IP + "连接的端口：" + m_Port);
            if (0 == m_Port)
            {
                m_ConnErrorCode = ConnectErrorCode.Lost;
                return;
            }
            //放入待回收的池子
            if (null != m_CurrentSocketClient)
            {
                m_CurrentSocketClient.Disconnect();
                m_SocketList.Add(m_CurrentSocketClient);
                m_CurrentSocketClient = null;
            }

            switch ((CONNECT_TYPE)type)
            {
                case CONNECT_TYPE.Connect_Type_UDP:
                    m_CurrentSocketClient = new UDPSocketClient();
                    break;
                case CONNECT_TYPE.Connect_Type_TCP:
                default:
                    m_CurrentSocketClient = new TCPSocketClient();
                    break;
            }
            m_eReason = SOCKET_ERROR.SOCKET_ERROR_NON;
            ++m_nCurSocketID;
            m_CurrentSocketClient.SetSink(this);
            m_CurrentSocketClient.SetSocketID(m_nCurSocketID);
            //连接服务器
            ReConnect(m_IP, m_Port);

        }

        //重新连接
        public void ReConnect(string ip, int port)
        {
            m_ConnErrorCode = ConnectErrorCode.Unknow;
            try
            {
                Debug.Log("连接");
                m_CurrentSocketClient.CreateConnection(ip, port);
            }
            catch (System.Net.Sockets.SocketException e)
            {
                m_ConnErrorCode = ConnectErrorCode.Error;
                Debug.LogError(e.Message);
            }

        }
        //断开连接
        public void Disconnect()
        {
            m_ConnectAction = null;
            if (m_LuaConnectCallback != null)
            {
                m_LuaConnectCallback.Dispose();
                m_LuaConnectCallback = null;
            }

            if (null != m_CurrentSocketClient)
            {
                m_CurrentSocketClient.Disconnect();
                m_SocketList.Add(m_CurrentSocketClient);
                m_CurrentSocketClient = null;
            }

            //断开连接后清除后续的包，不再接收
            m_Queue.Clear();

            //清除上次连接接收到的包
            QueueNode<IPackageData> node = null;
            do
            {
                node = m_oRecieveQueue.Pop();
                if (null != node)
                {
                    if (m_oRecieveRecycle.Size() < MAX_CACHE_SIZE)
                    {
                        node.item.Release(m_oRecieveMemPool);
                        m_oRecieveRecycle.Push(node);
                    }
                    else
                    {
                        node.item.Release(m_oRecieveMemPool);
                        node.item = null;

                    }
                }


            } while (null != node);


            isConnected = false;

        }

        //检查连接错误
        public void CheckConnectError()
        {
            if (m_ConnErrorCode != ConnectErrorCode.Unknow && m_ConnectAction != null)
            {
                isConnected = (ConnectErrorCode.Success == m_ConnErrorCode);
                ConnectErrorCode lastError = m_ConnErrorCode;
                SOCKET_ERROR eReason = m_eReason;


                m_ConnErrorCode = ConnectErrorCode.Unknow;
                m_ConnectAction((int)lastError,(int)eReason);
               
            }
        }

        public void SendMessage(byte serverID, UInt16 msgID, byte[] messageBuffer)
        {
            if (m_CurrentSocketClient != null)
            {

                QueueNode<IPackageData> node = m_oSendRecycle.Pop();
                if (null == node)
                {
                    node = new QueueNode<IPackageData>();
                    node.item = new PackageData();
                }
      

                PackageData packdata = (PackageData)node.item;
                packdata.serverID = serverID;
                packdata.msgID = msgID;
                packdata.sendData = messageBuffer;
                m_CurrentSocketClient.Send(node);
            }
        }

        void OnDestroy()
        {
            if(null!= m_MessageEventHandler)
            {
                m_MessageEventHandler.Dispose();
                m_MessageEventHandler = null;
            }
           
            Disconnect();

            foreach (ISocketClient socket in m_SocketList)
            {
                socket.Close();
            }

            m_SocketList.Clear();

            if(null!= m_oSendRecycle)
            {
                m_oSendRecycle.Release();
                m_oSendRecycle = null;
            }


            if (null != m_oRecieveRecycle)
            {
                m_oRecieveRecycle.Release();
                m_oRecieveRecycle = null;
            }

            if (null != m_oRecieveQueue)
            {
                m_oRecieveQueue.Release();
                m_oRecieveQueue = null;
            }

        }
    } 
}