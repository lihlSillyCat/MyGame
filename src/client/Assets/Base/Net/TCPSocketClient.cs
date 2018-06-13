/*******************************************************************
** 文件名:	TCPSocketClient.cs
** 版  权:	(C) 冰川网络有限公司
** 创建人:	许德纪
** 日  期:	2017.11.22
** 版  本:	1.0
** 描  述:	
** 应  用:  TCP类型Socket客户端

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System;
using System.Net;
using System.Collections;
using System.Net.Sockets;
using System.Threading;

namespace War.Base
{

   

    public class TCPSocketClient:  ISocketClient
    {

        private static readonly int MESSAGE_HEAD_LEN = 2;

        public const int BufferSize = 8192;
        
        private const int MaxDecompressSize = 1024 * 32;
        private const int CompressFlag = 0x8000;

        protected const UInt16 PacketLenSize = sizeof(UInt16);
        protected const UInt16 PacketIDSize = 0; //sizeof(UInt16);
        protected const UInt16 PacketHeaderSize = PacketLenSize + PacketIDSize;

        private Socket m_Connection;

        //socket 的回调
        private ISocketSink m_socketSink = null;


        private byte[] m_DecompressData;

        //是否启用压缩发包
        //private bool m_bEnableSendCompress = false;

        //连接ID
        private uint m_nSocketID = 0;


        protected class PacketBuffer
        {
            public int len = 0;
            public byte[] buffer = new byte[BufferSize];

            public byte[] swapBuffer = new byte[BufferSize];
        }




        //是否删除
        private bool m_Abort = false;

        private Thread m_ThreadReceive;
        //接收现场是否在运行
        private bool m_bRunReceive = false;

        private Thread m_ThreadSend;
        //发送线程是否在运行
        private bool m_bRunSend = false;

        //发送队列
        protected Queue m_QueueToSend = new Queue();


        public TCPSocketClient()
        {
            m_DecompressData = new byte[MaxDecompressSize];
        }

        //关闭
        public void Close()
        {
           // UnityEngine.Debug.LogWarningFormat("Close socket：");

            Disconnect();

            lock (m_QueueToSend.SyncRoot)
            {
                m_QueueToSend.Clear();
                Monitor.Pulse(m_QueueToSend.SyncRoot);
            }


            if (m_ThreadSend != null)
            {
                m_ThreadSend.Abort();
                m_ThreadSend = null;
            }

            if (m_ThreadReceive != null)
            {
                m_ThreadReceive.Abort();
                m_ThreadReceive = null;
            }

            if (m_Connection != null)
            {

                try
                {
                    m_Connection.Close();
                }
                catch (SocketException e)
                {
                    UnityEngine.Debug.LogErrorFormat("Close error:{0}", e.ToString());

                }

                m_Connection = null;
            }

            m_socketSink = null;
            m_DecompressData = null;



        }


        public bool CanClose()
        {
            return (false == m_bRunReceive) && (m_bRunSend == false);
        }


        public void CreateConnection(string serverAddress, int port)
        {
            if (m_Connection != null && m_Connection.Connected)
            {
                Disconnect();
            }

            IPAddress[] address = Dns.GetHostAddresses(serverAddress);
            AddressFamily addressFamily = AddressFamily.InterNetwork;
            if (address[0].AddressFamily == AddressFamily.InterNetworkV6)
            {
                addressFamily = AddressFamily.InterNetworkV6;
            }
            m_Connection = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp);
            m_Connection.NoDelay = true;

            m_Abort = false;

            m_Connection.BeginConnect(address, port, new AsyncCallback(Connected), m_Connection);
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
            lock (m_QueueToSend.SyncRoot)
            {
                m_QueueToSend.Enqueue(node);
                Monitor.Pulse(m_QueueToSend.SyncRoot);
            }
        }


        public void Disconnect()
        {

           //UnityEngine.Debug.LogWarningFormat("关闭TCPsocket：" );
            m_Abort = true;

            lock (m_QueueToSend.SyncRoot)
            {
                m_QueueToSend.Clear();
                Monitor.Pulse(m_QueueToSend.SyncRoot);
            }


            if (m_Connection != null && m_Connection.Connected)
            {
                try
                {
                    m_Connection.Shutdown(SocketShutdown.Both);
                }
                catch (SocketException e)
                {
                    UnityEngine.Debug.LogErrorFormat("Disconnect error:{0}", e.ToString());

                }
            }

        }


        private void Connected(IAsyncResult iar)
        {
            try
            {
                lock (m_QueueToSend.SyncRoot)
                {
                    m_QueueToSend.Clear();
                }

                m_Connection.EndConnect(iar);

                m_ThreadReceive = new Thread(new ThreadStart(ThreadReceive));
                m_ThreadSend = new Thread(new ThreadStart(ThreadSend));

              

                m_bRunReceive = true;
                m_bRunSend = true;

                m_ThreadReceive.Start();
                m_ThreadSend.Start();

                if (m_socketSink != null)
                {
                    m_socketSink.OnConnect(m_nSocketID);
                    //ConnectionCreated(this, new EventArgs());
                }
            }
            catch (SocketException)
            {
                if (m_socketSink != null)
                {
                    m_socketSink.OnConnectError(m_nSocketID, SOCKET_ERROR.SOCKET_ERROR_CONNECT_FAIL);
                }
            }
        }

        private bool IsConnected()
        {
            return m_Connection != null && m_Connection.Connected;
        }

        //更新
        public void Update()
        {

           
        }

        private void ThreadReceive()
        {
            var packetBuffer = new PacketBuffer();
            while (!m_Abort && IsConnected())
            {
                try
                {
                    int num = m_Connection.Receive(packetBuffer.buffer, packetBuffer.len, BufferSize - packetBuffer.len, SocketFlags.None);
                    if (num > 0)
                    {
                        packetBuffer.len += num;

                        int index = 0;
                        for (; index + PacketHeaderSize <= packetBuffer.len;)
                        {
                            UInt16 packetLen = BitConverter.ToUInt16(packetBuffer.buffer, index);
                            bool isCompress = (packetLen & CompressFlag) != 0;
                            packetLen &= (CompressFlag - 1);
                            if (index + packetLen + PacketLenSize > packetBuffer.len)
                            {
                                break;
                            }

                            //UInt16 packetId = 0;
                            //byte[] message;

                            //m_oRecieveDataSwap.Update();
                            //QueueNode<ByteData> node = null;

                            if (isCompress)
                            {
                                uint decompressLen = 0;
                                MiniLZO.Decompress(packetBuffer.buffer, index + PacketLenSize, packetLen,
                                    m_DecompressData, ref decompressLen);

                         
                                if (decompressLen < packetLen)
                                {
                                    UnityEngine.Debug.LogErrorFormat("error: packet decode size> org size:" + decompressLen + ">" + packetLen);
                                }

                                //回调数据包给上层处理
                                if(m_socketSink!=null)
                                {
                                    m_socketSink.OnReceive(m_nSocketID, m_DecompressData, 0,(int)decompressLen);
                                }

   
                            }
                            else
                            {

                                //回调数据包给上层处理
                                if (m_socketSink != null)
                                {
                                    m_socketSink.OnReceive(m_nSocketID, packetBuffer.buffer, index + PacketHeaderSize, (int)packetLen);
                                }


                            }

                            index += packetLen + PacketLenSize;
                        }

                        int remainSize = packetBuffer.len - index;
                        if (remainSize > 0)
                        {
                            Array.Copy(packetBuffer.buffer, index, packetBuffer.swapBuffer, 0, remainSize);

                            byte[] temp = packetBuffer.buffer;
                            packetBuffer.buffer = packetBuffer.swapBuffer;
                            packetBuffer.swapBuffer = temp;
                        }
                        packetBuffer.len = remainSize;
                    }
                    else
                    {
                        if (m_socketSink != null)
                        {
                            m_socketSink.OnClose(m_nSocketID);
                        }

                        break;
                    }
                }
                catch (SocketException e)
                {
                    m_Abort = true;
                    UnityEngine.Debug.LogErrorFormat("ThreadReceive error:{0}", e.ToString());

                    if (m_socketSink != null)
                    {
                        m_socketSink.OnConnectError(m_nSocketID, SOCKET_ERROR.SOCKET_ERROR_CONNECT_ERROR);
                    }

                       
                }
                catch (OverflowException e)
                {
                    m_Abort = true;
                    UnityEngine.Debug.LogErrorFormat("ThreadReceive error:{0}", e.ToString());

                    if (m_socketSink != null)
                    {
                        m_socketSink.OnConnectError(m_nSocketID, SOCKET_ERROR.SOCKET_ERROR_CONNECT_ERROR);
                    }   
                }
            }

            m_bRunReceive = false;
           
        }

        private void ThreadSend()
        {
            byte[] sendBuffer = new byte[BufferSize];

            while (!m_Abort && IsConnected())
            {

                // PacketSender packetSender;
                QueueNode<IPackageData> node = null;
                lock (m_QueueToSend.SyncRoot)
                {
                    while (m_QueueToSend.Count <= 0 && !m_Abort)
                    {
                        Monitor.Wait(m_QueueToSend.SyncRoot);
                    }

                    if (m_Abort)
                    {
                        break;
                    }
                    node = (QueueNode<IPackageData>)m_QueueToSend.Dequeue();
                }

                /*
                bool bSendCompress = false;
                int packetSize = PacketHeaderSize;
                if (m_bEnableSendCompress)
                {
                    //(1)先压缩
                    byte[] dataCompress;
                    MiniLZO.Compress(packetSender.data, out dataCompress);

                    //(2)判断长度是否比不压缩的更长
                    bSendCompress = dataCompress.Length < packetSender.data.Length;
                    if (bSendCompress)//发送压缩数据
                    {
                        //压缩的头两个字节,第16位指明是否压缩，剩下的15位是长度
                        int compressDataHead = dataCompress.Length | CompressFlag;
                        byte[] packetSizeBytes = BitConverter.GetBytes((ushort)compressDataHead);
                        Array.Copy(packetSizeBytes, 0, sendBuffer, 0, PacketLenSize);
                        Array.Copy(dataCompress, 0, sendBuffer, PacketLenSize, dataCompress.Length);
                        packetSize += dataCompress.Length;

                    }

                }
              
                //不被压缩的发原来的数据
                if(!bSendCompress)
                {
                    int compressDataHead = packetSender.data.Length;
                    byte[] packetSizeBytes = BitConverter.GetBytes((ushort)compressDataHead);
                    Array.Copy(packetSizeBytes, 0, sendBuffer, 0, PacketLenSize);
                    Array.Copy(packetSender.data, 0, sendBuffer, PacketLenSize, packetSender.data.Length);
                    packetSize += packetSender.data.Length;
                }


                SocketError error;
                m_Connection.Send(sendBuffer, 0, packetSize , SocketFlags.None, out error);

                if (error != SocketError.Success)
                {
                    UnityEngine.Debug.LogErrorFormat("Send message error:{0}", error.ToString());
                }
                */

                if (null != node)
                {
                    int nSize = node.item.Pack(sendBuffer, MESSAGE_HEAD_LEN);

                    byte[] packetSizeBytes = BitConverter.GetBytes(nSize);
                    Array.Copy(packetSizeBytes, 0, sendBuffer, 0, PacketLenSize);

                    SocketError error;
                    m_Connection.Send(sendBuffer, 0, nSize+ MESSAGE_HEAD_LEN, SocketFlags.None, out error);

                    //回调给上层,发送完成
                    if(null!=m_socketSink)
                    {
                        m_socketSink.OnFinishSendData(node);
                    }

                    if (error != SocketError.Success)
                    {
                        UnityEngine.Debug.LogErrorFormat("Send message error:{0}", error.ToString());
                    }

                }
  
            }

            m_bRunSend = false;
        }

    }
}