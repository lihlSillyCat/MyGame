/*******************************************************************
** 文件名:	UDPSocketClient.cs
** 版  权:	(C) 冰川网络有限公司
** 创建人:	许德纪
** 日  期:	2017.11.22
** 版  本:	1.0
** 描  述:	
** 应  用:  UDP类型Socket客户端

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/


using System;
using System.Collections;
using System.Text;
using System.Net;
using System.Net.Sockets;

using System.Threading;

namespace War.Base
{
    public class UDPSocketClient : ISocketClient
    {
        private static readonly DateTime utc_time = new DateTime(1970, 1, 1);

        public static UInt32 iclock()
        {
            return (UInt32)(Convert.ToInt64(DateTime.UtcNow.Subtract(utc_time).TotalMilliseconds) & 0xffffffff);
        }

        public enum cliEvent
        {
            Connected = 0,
            ConnectFailed = 1,
            Disconnect = 2,
            RcvMsg = 3,
        }

        private const UInt32 CONNECT_TIMEOUT = 5000;
        private const UInt32 RESEND_CONNECT = 500;
        private const UInt32 RECEIVE_TIMEOUT = 10000;
        private const UInt32 RESEND_HEART = 1000;
        private const UInt32 RECEIVE_FRAME_OUT = 300;

        private UdpClient mUdpClient;
        private IPEndPoint mIPEndPoint;
        private IPEndPoint mSvrEndPoint;

        private KCP mKcp;
        private bool mNeedUpdateFlag;
        private UInt32 mNextUpdateTime;

        private bool mInConnectStage;
        private bool mConnectSucceed;
        private UInt32 mConnectStartTime;

        //上次发包的时间
        private UInt32 mLastSendTime;

        //最后收包的时间
        private UInt32 mLastReceiveTime;

        //握手包内容
        private byte[] mHandshakePack = new byte[4] { 0, 0, 0, 0 };//

        //心跳包内容
        private byte[] mHeartPack = new byte[4] { 1, 1, 1, 1 };

        //断线包
        private byte[] mDisconnectPack = new byte[4] { 2, 2, 2, 2 };//{ 1, 1, 1, 1 };

        //当前的帧编号
        private UInt32 mCurFrame = 0;

        //最后一次收包时候的帧编号
        private UInt32 mLasRecieveFrame = 0;

        private UInt32 m_nDisconnectError = 0;

        //socket 的回调
        private ISocketSink m_socketSink = null;

        //发送组包缓冲区
        public const int BufferSize = 8192;
        public byte[] packbuffer = new byte[BufferSize];
        public byte[] recievebuffer = new byte[BufferSize * 2];

        //发送数据交换区
        private ThreadDataSwap m_oSendDataSwap;

        private SwitchQueue<byte[]> mRecvQueue = new SwitchQueue<byte[]>(128);

        //是否删除
        private bool m_Abort = false;

        private Thread m_ThreadSend;
        //发送线程是否在运行
        private bool m_bRunSend = false;

        //发送队列
        private Queue m_QueueToSend = new Queue();

        //连接ID
        private uint m_nSocketID = 0;

        public UDPSocketClient()
        {
            m_oSendDataSwap = new ThreadDataSwap();
            m_oSendDataSwap.Create();
        }

        //关闭
        public void Close()
        {
            // UnityEngine.Debug.LogWarningFormat("Close UDP socket：");
            Disconnect();

            //释放线程
            if (null != m_ThreadSend)
            {
                m_ThreadSend.Abort();
                m_ThreadSend = null;
            }

            //释放发包内存池
            if (null != m_oSendDataSwap)
            {
                m_oSendDataSwap.Release();
                m_oSendDataSwap = null;
            }

            //释放udp客户端
            if (null != mUdpClient)
            {
                mUdpClient.Close();
                mUdpClient = null;
            }

            //释放发送队列
            if (null != m_QueueToSend)
            {
                m_QueueToSend.Clear();
                m_QueueToSend = null;
            }

            //释放接收队列
            if (null != mRecvQueue)
            {
                mRecvQueue.Clear();
                mRecvQueue = null;
            }

            //重置状态
            reset_state();

            //清空缓存
            packbuffer = null;
            recievebuffer = null;
            mHandshakePack = null;
            mHeartPack = null;
            mDisconnectPack = null;

            //清空回调
            m_socketSink = null;
        }


        public bool CanClose()
        {
            return m_bRunSend == false;
        }


        //连接服务器
        public void CreateConnection(string serverAddress, int port)
        {
            if (null != mUdpClient)
            {
                mUdpClient.Close();
                mUdpClient = null;
            }

            IPAddress[] address = Dns.GetHostAddresses(serverAddress);
            mSvrEndPoint = new IPEndPoint(address[0], port);
            mUdpClient = new UdpClient(serverAddress, port);
            mUdpClient.Connect(mSvrEndPoint);

            reset_state();

            mInConnectStage = true;
            mConnectStartTime = iclock();
            mLastReceiveTime = mConnectStartTime;


            m_nDisconnectError = BitConverter.ToUInt32(mDisconnectPack, 0);

            mUdpClient.BeginReceive(ReceiveCallback, this);

            //启动发送线程
            m_bRunSend = true;
            m_ThreadSend = new Thread(new ThreadStart(ThreadSend));
            m_ThreadSend.Start();


        }

        //设置上层回调
        public void SetSink(ISocketSink sink)
        {
            m_socketSink = sink;
        }

        //设置连接的ID唯一标识连接号
        public void SetSocketID(uint nSocketID)
        {
            m_nSocketID = nSocketID;
        }

        //发送数据包
        public void Send(QueueNode<IPackageData> node)
        {

            int nSize = node.item.Pack(packbuffer, 0);
            Send(packbuffer, nSize);

            //回调上层,包处理完成
            if (m_socketSink != null)
            {
                m_socketSink.OnFinishSendData(node);
            }
        }

        public void Disconnect()
        {
            mConnectSucceed = false;
            m_Abort = true;

            if (null != m_QueueToSend)
            {
                lock (m_QueueToSend.SyncRoot)
                {
                    m_QueueToSend.Clear();
                    Monitor.Pulse(m_QueueToSend.SyncRoot);
                }
            }
        }

        public void Send(byte[] data, int nSize)
        {
            if (!mConnectSucceed)
            {
                return;

            }

            if (null == mKcp)
            {
                UnityEngine.Debug.LogWarning("TCP::Send()-->mKcp==null");
                return;
            }
            mKcp.Send(data, nSize);
            mNeedUpdateFlag = true;

        }

        void ReceiveCallback(IAsyncResult ar)
        {
            Byte[] data = (mIPEndPoint == null) ?
                mUdpClient.Receive(ref mIPEndPoint) :
                mUdpClient.EndReceive(ar, ref mIPEndPoint);

            if (null != data)
                OnData(data);

            if (mUdpClient != null)
            {
                // try to receive again.
                mUdpClient.BeginReceive(ReceiveCallback, this);
            }
        }

        //压入接收数据
        void OnData(byte[] buf)
        {
            mRecvQueue.Push(buf);
        }

        void reset_state()
        {
            mNeedUpdateFlag = false;
            mNextUpdateTime = 0;

            mInConnectStage = false;
            mConnectSucceed = false;
            mConnectStartTime = 0;
            mLastSendTime = 0;
            if (null != mRecvQueue)
            {
                mRecvQueue.Clear();
            }

            if (null != mKcp)
            {
                mKcp.Release();
                mKcp = null;
            }
        }

        string dump_bytes(byte[] buf, int size)
        {
            var sb = new StringBuilder(size * 2);
            for (var i = 0; i < size; i++)
            {
                sb.Append(buf[i]);
                sb.Append(" ");
            }
            return sb.ToString();
        }

        void init_kcp(UInt32 conv)
        {
            mKcp = new KCP(conv, (byte[] buf, int size) =>
            {
                PushSend(buf, size);
                //mUdpClient.Send(buf, size);
            });

            mKcp.NoDelay(1, 10, 2, 1);
        }

        //更新
        public void Update()
        {
            update(iclock());
        }

        void process_connect_packet()
        {
            mRecvQueue.Switch();

            if (!mRecvQueue.Empty())
            {
                var buf = mRecvQueue.Pop();

                UInt32 conv = 0;
                KCP.ikcp_decode32u(buf, 0, ref conv);

                if (conv <= 0)
                    throw new Exception("inlvaid connect back packet");

                init_kcp(conv);

                mInConnectStage = false;
                mConnectSucceed = true;

                if (m_socketSink != null)
                {
                    m_socketSink.OnConnect(m_nSocketID);
                }
            }
        }

        void process_recv_queue()
        {
            mRecvQueue.Switch();

            bool bHadRecv = !mRecvQueue.Empty();

            while (!mRecvQueue.Empty())
            {
                var buf = mRecvQueue.Pop();

                mNeedUpdateFlag = true;

                //跳过心跳包
                if (buf.Length <= 4)
                {
                    if (buf.Length == 4)
                    {
                        UInt32 errorCode = BitConverter.ToUInt32(buf, 0);
                        if (m_nDisconnectError == errorCode)
                        {
                            UnityEngine.Debug.LogWarning("recive disconnect data....");
                            if (m_socketSink != null)
                            {
                                mConnectSucceed = false;
                                m_socketSink.OnConnectError(m_nSocketID, SOCKET_ERROR.SOCKET_ERROR_SERVER_HANDSHAKE_DISCONNECT);
                            }
                            return;
                        }
                    }

                    continue;
                }

                mKcp.Input(buf);

                for (var size = mKcp.PeekSize(); size > 0; size = mKcp.PeekSize())
                {

                    int nRecvSize = mKcp.Recv(recievebuffer);
                    if (nRecvSize > 0)
                    {
                        if (m_socketSink != null)
                        {
                            m_socketSink.OnReceive(m_nSocketID, recievebuffer, 0, nRecvSize);
                        }
                    }
                }
            }

            //更新接收时间

            if (bHadRecv)
            {
                mLastReceiveTime = iclock();
                //推进一下发心跳包的时间
                mLasRecieveFrame = mCurFrame;
            }

        }

        bool connect_timeout(UInt32 current)
        {
            return current - mConnectStartTime > CONNECT_TIMEOUT;
        }


        bool receive_timeout(UInt32 current)
        {
            return (current - mLastReceiveTime > RECEIVE_TIMEOUT) && (mCurFrame - mLasRecieveFrame > RECEIVE_FRAME_OUT);
        }

        bool need_send_connect_packet(UInt32 current)
        {
            return current - mLastSendTime > RESEND_CONNECT;
        }

        //发心跳包时间间隔
        bool need_send_heart_packet(UInt32 current)
        {
            return current - mLastSendTime > RESEND_HEART;
        }

        //发送一个心跳包
        void send_heart_packet()
        {
            if (null != mUdpClient)
            {
                PushSend(mHeartPack, mHeartPack.Length);
            }
        }

        //发送一个断线包
        void send_disconnect_packet()
        {
            if (null != mUdpClient)
            {
                PushSend(mDisconnectPack, mDisconnectPack.Length);
            }
        }

        void send_hand_shake_packet()
        {
            if (null != mUdpClient)
            {
                PushSend(mHandshakePack, mHandshakePack.Length);
            }

        }

        void update(UInt32 current)
        {
            ++mCurFrame;

            if (mInConnectStage)
            {
                if (connect_timeout(current))
                {
                    if (m_socketSink != null)
                    {
                        Disconnect();
                        m_socketSink.OnConnectError(m_nSocketID, SOCKET_ERROR.SOCKET_ERROR_CONNECT_FAIL);
                    }
                    mInConnectStage = false;
                    return;
                }

                if (need_send_connect_packet(current))
                {
                    mLastSendTime = current;

                    //发送握手包
                    send_hand_shake_packet();
                }

                process_connect_packet();

                //初始化接收包时间
                mLastReceiveTime = current;
                mLasRecieveFrame = mCurFrame;

                return;
            }

            //bTest = false;
            if (mConnectSucceed)
            {
                //bTest = true;
                process_recv_queue();

                if (mNeedUpdateFlag || current >= mNextUpdateTime)
                {
                    mKcp.Update(current);
                    mNextUpdateTime = mKcp.Check(current);
                    mNeedUpdateFlag = false;
                }

                //检查是心跳接收是否超时(超时直接断开连接)
                if (receive_timeout(current))
                {
                    //发送断线包
                    send_disconnect_packet();

                    UnityEngine.Debug.LogWarning("send disconnect data....");

                    if (m_socketSink != null)
                    {
                        Disconnect();
                        m_socketSink.OnConnectError(m_nSocketID, SOCKET_ERROR.SOCKET_ERROR_CLIENT_HANDSHAKE_TIMEOUT);
                        return;
                    }
                }

                //发送心跳包
                if (need_send_heart_packet(current))
                {
                    mLastSendTime = current;
                    send_heart_packet();
                }
            }
        }

        //压入发送队列
        void PushSend(Byte[] data, int nLen)
        {
            //分配节点
            m_oSendDataSwap.Update();
            QueueNode<ByteData> node = m_oSendDataSwap.Aloc(nLen);
            Array.Copy(data, node.item.data, nLen);
            node.item.nLen = nLen;

            //压入发送队列
            lock (m_QueueToSend.SyncRoot)
            {
                m_QueueToSend.Enqueue(node);
                Monitor.Pulse(m_QueueToSend.SyncRoot);
            }
        }

        private void ThreadSend()
        {
            while (!m_Abort)
            {
                QueueNode<ByteData> node = null;
                lock (m_QueueToSend.SyncRoot)
                {
                    if (m_QueueToSend.Count <= 0 && !m_Abort)
                    {
                        Monitor.Wait(m_QueueToSend.SyncRoot, 2000);

                    }

                    if (m_Abort)
                    {
                        break;
                    }

                    if (m_QueueToSend.Count > 0)
                    {
                        node = (QueueNode<ByteData>)m_QueueToSend.Dequeue();
                    }
                    else
                    {
                        node = null;
                    }
                }

                //发送消息
                if (null != node)
                {
                    int nNum = mUdpClient.Send(node.item.data, node.item.nLen);

                    if (nNum != node.item.nLen)
                    {
                        UnityEngine.Debug.LogErrorFormat("Send message error: num==", nNum.ToString(), node.item.nLen.ToString());
                    }

                    m_oSendDataSwap.Recycle(ref node);
                }
                else  //没有消息的时候，发送一次心跳包
                {

                    if (mConnectSucceed)
                    {
                        mUdpClient.Send(mHeartPack, mHeartPack.Length);
                    }
                    else
                    {
                        mUdpClient.Send(mHandshakePack, mHandshakePack.Length);
                    }
                }
            }

            m_bRunSend = false;
        }
    }
}
