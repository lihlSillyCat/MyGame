using System;
using System.Net;
using System.Collections;
using System.Net.Sockets;
using System.Threading;

namespace War.Base
{
    public class SocketClient
    {
        public const int BufferSize = 8192;
        
        private const int MaxDecompressSize = 1024 * 32;
        private const int CompressFlag = 0x8000;

        protected const UInt16 PacketLenSize = sizeof(UInt16);
        protected const UInt16 PacketIDSize = 0; //sizeof(UInt16);
        protected const UInt16 PacketHeaderSize = PacketLenSize + PacketIDSize;

        private Socket m_Connection;

        public event EventHandler<SocketMessageReceived> SocketMessageReceived;
        public event EventHandler<EventArgs> ConnectionCreated;
        public event EventHandler CloseHandler;
        public event EventHandler ConnectError;
        public event EventHandler ReconnectHandle;

        private byte[] m_DecompressData;

        //是否启用压缩发包
        private bool m_bEnableSendCompress = false;

        protected class PacketBuffer
        {
            public int len = 0;
            public byte[] buffer = new byte[BufferSize];

            public byte[] swapBuffer = new byte[BufferSize];
        }

        protected struct PacketSender
        {
            public UInt16 packetId;
            public byte[] data;

            public PacketSender(byte[] data)
            {
                this.packetId = 0;
                this.data = data;
            }
        }

        private Thread m_ThreadReceive;
        private Thread m_ThreadSend;

        protected Queue m_QueueToSend = new Queue();

        private bool m_Abort = false;

        public SocketClient()
        {
            m_DecompressData = new byte[MaxDecompressSize];
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

        public void Disconnect()
        {
            m_Abort = true;

            lock (m_QueueToSend.SyncRoot)
            {
                Monitor.Pulse(m_QueueToSend.SyncRoot);
            }

            if (m_ThreadSend != null)
            {
                m_ThreadSend.Join();
                m_ThreadSend = null;
            }

            if (m_ThreadReceive != null)
            {
                m_ThreadReceive.Abort();
                m_ThreadReceive = null;
            }

            if (m_Connection != null && m_Connection.Connected)
            {
                try
                {
                    m_Connection.Shutdown(SocketShutdown.Both);
                    m_Connection.Close();
                    m_Connection = null;
                }
                catch
                {
                    m_Connection = null;
                }
            }
        }

        public void SendMessageToServer(byte[] data)
        {
            if (m_Connection == null)
            {
                if (ReconnectHandle != null)
                {
                    ReconnectHandle(this, new EventArgs());
                }
                return;
            }
            if (!m_Connection.Connected)
            {
                if (ReconnectHandle != null)
                {
                    ReconnectHandle(this, new EventArgs());
                }
                return;
            }

            lock (m_QueueToSend.SyncRoot)
            {
                m_QueueToSend.Enqueue(new PacketSender(data));
                Monitor.Pulse(m_QueueToSend.SyncRoot);
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

                m_ThreadReceive.Start();
                m_ThreadSend.Start();

                if (ConnectionCreated != null)
                {
                    ConnectionCreated(this, new EventArgs());
                }
            }
            catch (SocketException)
            {
                if (ConnectError != null)
                {
                    ConnectError(this, new EventArgs());
                }
            }
        }

        private bool IsConnected()
        {
            return m_Connection != null && m_Connection.Connected;
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

                            UInt16 packetId = 0;
                            byte[] message;

                            if (isCompress)
                            {
                                uint decompressLen = 0;
                                MiniLZO.Decompress(packetBuffer.buffer, index + PacketLenSize, packetLen,
                                    m_DecompressData, ref decompressLen);

                                //packetId = BitConverter.ToUInt16(m_DecompressData, 0);

                                message = new byte[decompressLen];
                                Array.Copy(m_DecompressData, message, message.Length);
                            }
                            else
                            {
                                //packetId = BitConverter.ToUInt16(packetBuffer.buffer, index + PacketLenSize);

                                int messageLength = packetLen;
                                message = new byte[messageLength];

                                Array.Copy(packetBuffer.buffer, index + PacketHeaderSize, message, 0, messageLength);
                            }

                            if (SocketMessageReceived != null)
                            {
                                SocketMessageReceived(this,
                                    new SocketMessageReceived(packetId, message, (UInt16)message.Length));
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
                        if (CloseHandler != null)
                        {
                            CloseHandler(this, new EventArgs());
                        }
                        break;
                    }
                }
                catch (SocketException e)
                {
                    m_Abort = true;
                    UnityEngine.Debug.LogErrorFormat("ThreadReceive error:{0}", e.ToString());
                    ConnectError(this, new EventArgs());
                }
                catch (OverflowException e)
                {
                    m_Abort = true;
                    UnityEngine.Debug.LogErrorFormat("ThreadReceive error:{0}", e.ToString());
                    ConnectError(this, new EventArgs());
                }
            }
        }

        private void ThreadSend()
        {
            byte[] sendBuffer = new byte[BufferSize];

            while (!m_Abort && IsConnected())
            {
                PacketSender packetSender;
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

                    packetSender = (PacketSender)m_QueueToSend.Dequeue();
                }


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
            }
        }
    }
}