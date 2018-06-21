using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace War.Base
{


    public class KCP
    {
        public const int IKCP_RTO_NDL = 30;  // no delay min rto
        public const int IKCP_RTO_MIN = 100; // normal min rto
        public const int IKCP_RTO_DEF = 200;
        public const int IKCP_RTO_MAX = 60000;
        public const int IKCP_CMD_PUSH = 81; // cmd: push data
        public const int IKCP_CMD_ACK = 82; // cmd: ack
        public const int IKCP_CMD_WASK = 83; // cmd: window probe (ask)
        public const int IKCP_CMD_WINS = 84; // cmd: window size (tell)
        public const int IKCP_ASK_SEND = 1;  // need to send IKCP_CMD_WASK
        public const int IKCP_ASK_TELL = 2;  // need to send IKCP_CMD_WINS
        public const int IKCP_WND_SND = 32;
        public const int IKCP_WND_RCV = 32;
        public const int IKCP_MTU_DEF = 1400;
        public const int IKCP_ACK_FAST = 3;
        public const int IKCP_INTERVAL = 100;
        public const int IKCP_OVERHEAD = 24;
        public const int IKCP_DEADLINK = 10;
        public const int IKCP_THRESH_INIT = 2;
        public const int IKCP_THRESH_MIN = 2;
        public const int IKCP_PROBE_INIT = 7000;   // 7 secs to probe window size
        public const int IKCP_PROBE_LIMIT = 120000; // up to 120 secs to probe window

        //最大报文缓存对象
        public const int IKCP_MAX_SEGMENT_OBJECT = 256;


        // encode 8 bits unsigned int
        public static int ikcp_encode8u(byte[] p, int offset, byte c)
        {
            p[0 + offset] = c;
            return 1;
        }

        // decode 8 bits unsigned int
        public static int ikcp_decode8u(byte[] p, int offset, ref byte c)
        {
            c = p[0 + offset];
            return 1;
        }

        /* encode 16 bits unsigned int (lsb) */
        public static int ikcp_encode16u(byte[] p, int offset, UInt16 w)
        {
            p[0 + offset] = (byte)(w >> 0);
            p[1 + offset] = (byte)(w >> 8);
            return 2;
        }

        /* decode 16 bits unsigned int (lsb) */
        public static int ikcp_decode16u(byte[] p, int offset, ref UInt16 c)
        {
            UInt16 result = 0;
            result |= (UInt16)p[0 + offset];
            result |= (UInt16)(p[1 + offset] << 8);
            c = result;
            return 2;
        }

        /* encode 32 bits unsigned int (lsb) */
        public static int ikcp_encode32u(byte[] p, int offset, UInt32 l)
        {
            p[0 + offset] = (byte)(l >> 0);
            p[1 + offset] = (byte)(l >> 8);
            p[2 + offset] = (byte)(l >> 16);
            p[3 + offset] = (byte)(l >> 24);
            return 4;
        }

        /* decode 32 bits unsigned int (lsb) */
        public static int ikcp_decode32u(byte[] p, int offset, ref UInt32 c)
        {
            UInt32 result = 0;
            result |= (UInt32)p[0 + offset];
            result |= (UInt32)(p[1 + offset] << 8);
            result |= (UInt32)(p[2 + offset] << 16);
            result |= (UInt32)(p[3 + offset] << 24);
            c = result;
            return 4;
        }

        /*
        public static byte[] slice(byte[] p, int start, int stop) {
            var bytes = new byte[stop - start];
            Array.Copy(p, start, bytes, 0, bytes.Length);
            return bytes;
        }

        public static T[] slice<T>(T[] p, int start, int stop) {
            var arr = new T[stop - start];
            var index = 0;
            for (var i = start; i < stop; i++)
            {
                arr[index] = p[i];
                index++;
            }

            return arr;
        }

        public static byte[] append(byte[] p, byte c) {
            var bytes = new byte[p.Length + 1];
            Array.Copy(p, bytes, p.Length);
            bytes[p.Length] = c;
            return bytes;
        }

        public static T[] append<T>(T[] p, T c) {
            var arr = new T[p.Length + 1];
            for (var i = 0; i < p.Length; i++)
                arr[i] = p[i];
            arr[p.Length] = c;
            return arr;
        }

        public static T[] append<T>(T[] p, T[] cs)
        {
            var arr = new T[p.Length + cs.Length];
            for (var i = 0; i < p.Length; i++)
                arr[i] = p[i];
            for (var i = 0; i < cs.Length; i++ )
                arr[p.Length+i] = cs[i];
            return arr;
        }
           */

        static UInt32 _imin_(UInt32 a, UInt32 b)
        {
            return a <= b ? a : b;
        }

        static UInt32 _imax_(UInt32 a, UInt32 b)
        {
            return a >= b ? a : b;
        }

        static UInt32 _ibound_(UInt32 lower, UInt32 middle, UInt32 upper)
        {
            return _imin_(_imax_(lower, middle), upper);
        }

        static Int32 _itimediff(UInt32 later, UInt32 earlier)
        {
            return ((Int32)(later - earlier));
        }



        // KCP Segment Definition
        internal class Segment
        {
            internal UInt32 conv = 0;
            internal UInt32 cmd = 0;
            internal UInt32 frg = 0;
            internal UInt32 wnd = 0;
            internal UInt32 ts = 0;
            internal UInt32 sn = 0;
            internal UInt32 una = 0;
            internal UInt32 resendts = 0;
            internal UInt32 rto = 0;
            internal UInt32 fastack = 0;
            internal UInt32 xmit = 0;
            internal QueueNode<ByteData> data;

            internal Segment()
            {
                data = null;
            }

            internal Segment(int size)
            {
                data = new QueueNode<ByteData>();// byte[size];
                data.item = new ByteData();
                data.item.data = new byte[size];
                data.item.nLen = size;
            }


            internal void Clear()
            {
                ResetState();
                data = null;
            }

            internal void ResetState()
            {
                conv = 0;
                cmd = 0;
                frg = 0;
                wnd = 0;
                ts = 0;
                sn = 0;
                una = 0;
                resendts = 0;
                rto = 0;
                fastack = 0;
                xmit = 0;

            }

            // encode a segment into buffer
            internal int encode(byte[] ptr, int offset)
            {

                var offset_ = offset;

                offset += ikcp_encode32u(ptr, offset, conv);
                offset += ikcp_encode8u(ptr, offset, (byte)cmd);
                offset += ikcp_encode8u(ptr, offset, (byte)frg);
                offset += ikcp_encode16u(ptr, offset, (UInt16)wnd);
                offset += ikcp_encode32u(ptr, offset, ts);
                offset += ikcp_encode32u(ptr, offset, sn);
                offset += ikcp_encode32u(ptr, offset, una);
                offset += ikcp_encode32u(ptr, offset, (UInt32)data.item.nLen);

                return offset - offset_;
            }
        }

        // kcp members.
        UInt32 conv; UInt32 mtu; UInt32 mss;
        //UInt32 state;
        UInt32 snd_una; UInt32 snd_nxt; UInt32 rcv_nxt;
        //UInt32 ts_recent; UInt32 ts_lastack;
        UInt32 ssthresh;
        UInt32 rx_rttval; UInt32 rx_srtt; UInt32 rx_rto; UInt32 rx_minrto;
        UInt32 snd_wnd; UInt32 rcv_wnd; UInt32 rmt_wnd; UInt32 cwnd; UInt32 probe;
        UInt32 current; UInt32 interval; UInt32 ts_flush; UInt32 xmit;
        UInt32 nodelay; UInt32 updated;
        UInt32 ts_probe; UInt32 probe_wait;
        UInt32 dead_link; UInt32 incr;

        List<QueueNode<Segment>> snd_queue = new List<QueueNode<Segment>>();
        List<QueueNode<Segment>> rcv_queue = new List<QueueNode<Segment>>();
        List<QueueNode<Segment>> snd_buf = new List<QueueNode<Segment>>();
        List<QueueNode<Segment>> rcv_buf = new List<QueueNode<Segment>>();

        /*
        Segment[] snd_queue = new Segment[0];
        Segment[] rcv_queue = new Segment[0];
        Segment[] snd_buf = new Segment[0];
        Segment[] rcv_buf = new Segment[0]; 
        */



        //UInt32[] acklist = new UInt32[0];
        List<UInt32> acklist = new List<UInt32>();

        byte[] buffer;
        Int32 fastresend;
        Int32 nocwnd;
        //Int32 logmask;
        // buffer, size
        Action<byte[], int> output;

        //临时变量减少new
        Segment m_oTeampSeg = new Segment(0);


        //报文包对象池
        private ThreadQueue<Segment> m_oSegmentPool;

        //内存池
        private IMemPool m_oMemPool;

        // create a new kcp control object, 'conv' must equal in two endpoint
        // from the same connection.
        public KCP(UInt32 conv_, Action<byte[], int> output_)
        {
            conv = conv_;
            snd_wnd = IKCP_WND_SND;
            rcv_wnd = IKCP_WND_RCV;
            rmt_wnd = IKCP_WND_RCV;
            mtu = IKCP_MTU_DEF;
            mss = mtu - IKCP_OVERHEAD;

            rx_rto = IKCP_RTO_DEF;
            rx_minrto = IKCP_RTO_MIN;
            interval = IKCP_INTERVAL;
            ts_flush = IKCP_INTERVAL;
            ssthresh = IKCP_THRESH_INIT;
            dead_link = IKCP_DEADLINK;
            buffer = new byte[(mtu + IKCP_OVERHEAD) * 3];
            output = output_;

            //创建报文包对象池
            m_oSegmentPool = new ThreadQueue<Segment>();
            m_oSegmentPool.Create();

            //内存池
            m_oMemPool = new MemPool();
            m_oMemPool.Create();

            //
        }

        //释放
        public void Release()
        {
            if (null != m_oSegmentPool)
            {
                m_oSegmentPool.Release();
                m_oSegmentPool = null;
            }

            if (null != m_oMemPool)
            {
                m_oMemPool.Release();
                m_oMemPool = null;
            }

        }

        private QueueNode<Segment> AlocSegmentNode(int nSize)
        {
            QueueNode<Segment> node = m_oSegmentPool.Pop();
            if (null == node)
            {
                node = new QueueNode<Segment>();
                node.item = new Segment();

            }
            else
            {
                node.item.Clear();
            }

            //分配byte data
            node.item.data = m_oMemPool.Aloc(nSize);
            node.item.data.item.nLen = nSize;

            return node;
        }

        private void RecycleSegmentNode(QueueNode<Segment> node)
        {
            //回收data的内存
            m_oMemPool.Recycle(ref node.item.data);
            node.item.data = null;
            node.next = null;


            if (null != node)
            {
                if (IKCP_MAX_SEGMENT_OBJECT > m_oSegmentPool.Size())
                {
                    m_oSegmentPool.Push(node);
                }

            }
        }


        void RecycleSegmentRange(List<QueueNode<Segment>> listSegment, int nStart, int nEnd)
        {
            int nCount = listSegment.Count;
            if (nCount < nEnd)
            {
                UnityEngine.Debug.LogError("RecycleSegmentRange:nCount: " + nCount + "nEnd: " + nEnd);
                return;
            }

            //单个回收
            for (int i = nStart; i < nEnd; ++i)
            {
                RecycleSegmentNode(listSegment[i]);
            }

            //清除范围
            listSegment.RemoveRange(nStart, nEnd - nStart);
        }

        // check the size of next message in the recv queue
        public int PeekSize()
        {

            int nCount = rcv_queue.Count;

            if (0 == nCount) return -1;

            Segment seg = rcv_queue[0].item;

            if (0 == seg.frg) return seg.data.item.nLen;

            if (nCount < seg.frg + 1) return -1;

            int length = 0;

            for (int i = 0; i < nCount; ++i)
            {
                seg = rcv_queue[i].item;
                length += seg.data.item.nLen;
                if (0 == seg.frg)
                    break;
            }

            /*
            foreach (var item in rcv_queue) {
                length += item.data.item.nLen;
                if (0 == item.frg)
                    break;
            }*/

            return length;
        }

        // move available data from rcv_buf -> rcv_queue
        public void MoveRecvBuff2RecvQueue()
        {

            int count = 0;
            int nLen = rcv_buf.Count;
            QueueNode<Segment> node = null;
            Segment seg = null;
            for (int i = 0; i < nLen; ++i)
            {
                node = rcv_buf[i];
                seg = node.item;
                if (seg.sn == rcv_nxt && rcv_queue.Count < rcv_wnd)
                {
                    //rcv_queue = append<Segment>(rcv_queue, seg);
                    rcv_queue.Add(node);
                    rcv_nxt++;
                    count++;
                }
                else
                {
                    break;
                }
            }

            /*
            foreach (var seg in rcv_buf) {
                if (seg.sn == rcv_nxt && rcv_queue.Length < rcv_wnd) {
                    rcv_queue = append<Segment>(rcv_queue, seg);
                    rcv_nxt++;
                    count++;
                } else {
                    break;
                }
            }
            */

            if (0 < count)
            {
                // rcv_buf = slice<Segment>(rcv_buf, count, rcv_buf.Length);
                rcv_buf.RemoveRange(0, count);
            }

        }

        // user/upper level recv: returns size, returns below zero for EAGAIN
        public int Recv(byte[] buffer)
        {

            int nLen = rcv_queue.Count;
            if (0 == nLen) return -1;

            var peekSize = PeekSize();
            if (0 > peekSize) return -2;

            if (peekSize > buffer.Length) return -3;

            var fast_recover = false;
            if (nLen >= rcv_wnd) fast_recover = true;

            // merge fragment.
            var count = 0;
            var n = 0;
            Segment seg = null;
            for (int i = 0; i < nLen; ++i)
            {
                seg = rcv_queue[i].item;
                Array.Copy(seg.data.item.data, 0, buffer, n, seg.data.item.nLen);
                n += seg.data.item.nLen;
                count++;
                if (0 == seg.frg) break;
            }

            /*
            foreach (var seg in rcv_queue) {
                Array.Copy(seg.data.item.data, 0, buffer, n, seg.data.item.nLen);
                n += seg.data.item.nLen;
                count++;
                if (0 == seg.frg) break;
            }
            */

            if (0 < count)
            {
                //rcv_queue = slice<Segment>(rcv_queue, count, rcv_queue.Length);
                RecycleSegmentRange(rcv_queue, 0, count);
            }

            // move available data from rcv_buf -> rcv_queue
            MoveRecvBuff2RecvQueue();

            // fast recover
            if (rcv_queue.Count < rcv_wnd && fast_recover)
            {
                // ready to send back IKCP_CMD_WINS in ikcp_flush
                // tell remote my window size
                probe |= IKCP_ASK_TELL;
            }

            return n;
        }

        // user/upper level send, returns below zero for error
        public int Send(byte[] buffer, int nBufferSize)
        {

            if (0 == nBufferSize) return -1;

            var count = 0;

            if (nBufferSize < mss)
                count = 1;
            else
                count = (int)(nBufferSize + mss - 1) / (int)mss;

            if (255 < count) return -2;

            if (0 == count) count = 1;

            var offset = 0;

            QueueNode<Segment> node = null;
            Segment seg = null;

            for (var i = 0; i < count; i++)
            {
                var size = 0;
                if (nBufferSize - offset > mss)
                    size = (int)mss;
                else
                    size = nBufferSize - offset;


                node = AlocSegmentNode(size);
                seg = node.item;
                Array.Copy(buffer, offset, seg.data.item.data, 0, size);
                offset += size;
                seg.frg = (UInt32)(count - i - 1);
                snd_queue.Add(node);

                /*
                var seg = new Segment(size);
                Array.Copy(buffer, offset, seg.data.item.data, 0, size);
                offset += size;
                seg.frg = (UInt32)(count - i - 1);
                snd_queue = append<Segment>(snd_queue, seg);
                */

            }

            return 0;
        }

        // update ack.
        void update_ack(Int32 rtt)
        {
            if (0 == rx_srtt)
            {
                rx_srtt = (UInt32)rtt;
                rx_rttval = (UInt32)rtt / 2;
            }
            else
            {
                Int32 delta = (Int32)((UInt32)rtt - rx_srtt);
                if (0 > delta) delta = -delta;

                rx_rttval = (3 * rx_rttval + (uint)delta) / 4;
                rx_srtt = (UInt32)((7 * rx_srtt + rtt) / 8);
                if (rx_srtt < 1) rx_srtt = 1;
            }

            var rto = (int)(rx_srtt + _imax_(1, 4 * rx_rttval));
            rx_rto = _ibound_(rx_minrto, (UInt32)rto, IKCP_RTO_MAX);
        }

        void shrink_buf()
        {
            if (snd_buf.Count > 0)
                snd_una = snd_buf[0].item.sn;
            else
                snd_una = snd_nxt;
        }

        void parse_ack(UInt32 sn)
        {

            if (_itimediff(sn, snd_una) < 0 || _itimediff(sn, snd_nxt) >= 0) return;

            //var index = 0;
            int nCount = snd_buf.Count;
            QueueNode<Segment> node = null;
            Segment seg = null;
            for (int i = 0; i < nCount; ++i)
            {
                node = snd_buf[i];
                seg = node.item;
                if (sn == seg.sn)
                {
                    // snd_buf = append<Segment>(slice<Segment>(snd_buf, 0, index), slice<Segment>(snd_buf, index + 1, snd_buf.Length));
                    snd_buf.RemoveAt(i);
                    RecycleSegmentNode(node);
                    break;
                }
                else
                {
                    seg.fastack++;
                }

                //index++;

            }

            /*
             foreach (var seg in snd_buf) {
                 if (sn == seg.sn)
                 {
                     snd_buf = append<Segment>(slice<Segment>(snd_buf, 0, index), slice<Segment>(snd_buf, index + 1, snd_buf.Length));
                     break;
                 }
                 else
                 {
                     seg.fastack++;
                 }

                 index++;
             }
             */
        }

        void parse_una(UInt32 una)
        {
            var count = 0;
            int nCount = snd_buf.Count;
            QueueNode<Segment> node = null;
            Segment seg = null;
            for (int i = 0; i < nCount; ++i)
            {
                node = snd_buf[i];
                seg = node.item;
                if (_itimediff(una, seg.sn) > 0)
                    count++;
                else
                    break;

            }

            if (0 < count)
            {

                //snd_buf = slice<Segment>(snd_buf, count, snd_buf.Length);
                RecycleSegmentRange(snd_buf, 0, count);

            }





            /*
                    foreach (var seg in snd_buf) {
                if (_itimediff(una, seg.sn) > 0)
                    count++;
                else
                    break;
            }

            if (0 < count) snd_buf = slice<Segment>(snd_buf, count, snd_buf.Length);
            */
        }

        void ack_push(UInt32 sn, UInt32 ts)
        {
            //acklist = append<UInt32>(acklist, new UInt32[2]{sn, ts});
            acklist.Add(sn);
            acklist.Add(ts);
        }

        void ack_get(int p, ref UInt32 sn, ref UInt32 ts)
        {
            sn = acklist[p * 2 + 0];
            ts = acklist[p * 2 + 1];
        }

        void parse_data(QueueNode<Segment> newNode)
        {
            var sn = newNode.item.sn;
            if (_itimediff(sn, rcv_nxt + rcv_wnd) >= 0 || _itimediff(sn, rcv_nxt) < 0) return;

            var n = rcv_buf.Count - 1;
            var after_idx = -1;
            var repeat = false;
            QueueNode<Segment> node = null;
            Segment seg = null;
            for (int i = n; i >= 0; i--)
            {
                node = rcv_buf[i];
                seg = node.item;
                if (seg.sn == sn)
                {
                    repeat = true;
                    break;
                }

                if (_itimediff(sn, seg.sn) > 0)
                {
                    after_idx = i;
                    break;
                }
            }

            if (!repeat)
            {
                /*
                if (after_idx == -1)
                {
                    //rcv_buf = append<Segment>(new Segment[1] { newseg }, rcv_buf);
                    rcv_buf.Insert(0, newNode);
                }

                else
                {
                    //rcv_buf = append<Segment>(slice<Segment>(rcv_buf, 0, after_idx + 1), append<Segment>(new Segment[1] { newseg }, slice<Segment>(rcv_buf, after_idx + 1, rcv_buf.Length)));

                    rcv_buf.Insert(after_idx + 1, newNode);

                }
                */



                rcv_buf.Insert(after_idx + 1, newNode);

            }

            // move available data from rcv_buf -> rcv_queue
            MoveRecvBuff2RecvQueue();
            /*
                        // move available data from rcv_buf -> rcv_queue
                        var count = 0;
                    foreach (var seg in rcv_buf) {
                        if (seg.sn == rcv_nxt && rcv_queue.Length < rcv_wnd)
                        {
                            rcv_queue = append<Segment>(rcv_queue, seg);
                            rcv_nxt++;
                            count++;
                        }
                        else 
                        {
                            break;
                        }
                    }

                    if (0 < count) {
                        rcv_buf = slice<Segment>(rcv_buf, count, rcv_buf.Length);
                    }
                    */
        }

        // when you received a low level packet (eg. UDP packet), call it
        public int Input(byte[] data)
        {

            var s_una = snd_una;
            if (data.Length < IKCP_OVERHEAD) return 0;

            var offset = 0;

            while (true)
            {
                UInt32 ts = 0;
                UInt32 sn = 0;
                UInt32 length = 0;
                UInt32 una = 0;
                UInt32 conv_ = 0;

                UInt16 wnd = 0;

                byte cmd = 0;
                byte frg = 0;

                if (data.Length - offset < IKCP_OVERHEAD) break;

                offset += ikcp_decode32u(data, offset, ref conv_);

                if (conv != conv_) return -1;

                offset += ikcp_decode8u(data, offset, ref cmd);
                offset += ikcp_decode8u(data, offset, ref frg);
                offset += ikcp_decode16u(data, offset, ref wnd);
                offset += ikcp_decode32u(data, offset, ref ts);
                offset += ikcp_decode32u(data, offset, ref sn);
                offset += ikcp_decode32u(data, offset, ref una);
                offset += ikcp_decode32u(data, offset, ref length);

                if (data.Length - offset < length) return -2;

                switch (cmd)
                {
                    case IKCP_CMD_PUSH:
                    case IKCP_CMD_ACK:
                    case IKCP_CMD_WASK:
                    case IKCP_CMD_WINS:
                        break;
                    default:
                        return -3;
                }

                rmt_wnd = (UInt32)wnd;
                parse_una(una);
                shrink_buf();

                if (IKCP_CMD_ACK == cmd)
                {
                    if (_itimediff(current, ts) >= 0)
                    {
                        update_ack(_itimediff(current, ts));
                    }
                    parse_ack(sn);
                    shrink_buf();
                }
                else if (IKCP_CMD_PUSH == cmd)
                {
                    if (_itimediff(sn, rcv_nxt + rcv_wnd) < 0)
                    {
                        ack_push(sn, ts);
                        if (_itimediff(sn, rcv_nxt) >= 0)
                        {
                            //var seg = new Segment((int)length);

                            QueueNode<Segment> node = AlocSegmentNode((int)length);
                            Segment seg = node.item;

                            seg.conv = conv_;
                            seg.cmd = (UInt32)cmd;
                            seg.frg = (UInt32)frg;
                            seg.wnd = (UInt32)wnd;
                            seg.ts = ts;
                            seg.sn = sn;
                            seg.una = una;

                            if (length > 0) Array.Copy(data, offset, seg.data.item.data, 0, length);

                            parse_data(node);
                        }
                    }
                }
                else if (IKCP_CMD_WASK == cmd)
                {
                    // ready to send back IKCP_CMD_WINS in Ikcp_flush
                    // tell remote my window size
                    probe |= IKCP_ASK_TELL;
                }
                else if (IKCP_CMD_WINS == cmd)
                {
                    // do nothing
                }
                else
                {
                    return -3;
                }

                offset += (int)length;
            }

            if (_itimediff(snd_una, s_una) > 0)
            {
                if (cwnd < rmt_wnd)
                {
                    var mss_ = mss;
                    if (cwnd < ssthresh)
                    {
                        cwnd++;
                        incr += mss_;
                    }
                    else
                    {
                        if (incr < mss_)
                        {
                            incr = mss_;
                        }
                        incr += (mss_ * mss_) / incr + (mss_ / 16);
                        if ((cwnd + 1) * mss_ <= incr) cwnd++;
                    }
                    if (cwnd > rmt_wnd)
                    {
                        cwnd = rmt_wnd;
                        incr = rmt_wnd * mss_;
                    }
                }
            }

            return 0;
        }

        Int32 wnd_unused()
        {
            if (rcv_queue.Count < rcv_wnd)
                return (Int32)(int)rcv_wnd - rcv_queue.Count;
            return 0;
        }

        // flush pending data
        void flush()
        {
            var current_ = current;
            var change = 0;
            var lost = 0;

            if (0 == updated) return;

            Segment seg = m_oTeampSeg;// new Segment(0);
            seg.ResetState();
            seg.conv = conv;
            seg.cmd = IKCP_CMD_ACK;
            seg.wnd = (UInt32)wnd_unused();
            seg.una = rcv_nxt;

            // flush acknowledges
            var count = acklist.Count / 2;
            var offset = 0;
            for (var i = 0; i < count; i++)
            {
                if (offset + IKCP_OVERHEAD > mtu)
                {
                    output(buffer, offset);
                    //Array.Clear(buffer, 0, offset);
                    offset = 0;
                }
                ack_get(i, ref seg.sn, ref seg.ts);
                offset += seg.encode(buffer, offset);
            }
            // acklist = new UInt32[0];
            acklist.Clear();

            // probe window size (if remote window size equals zero)
            if (0 == rmt_wnd)
            {
                if (0 == probe_wait)
                {
                    probe_wait = IKCP_PROBE_INIT;
                    ts_probe = current + probe_wait;
                }
                else
                {
                    if (_itimediff(current, ts_probe) >= 0)
                    {
                        if (probe_wait < IKCP_PROBE_INIT)
                            probe_wait = IKCP_PROBE_INIT;
                        probe_wait += probe_wait / 2;
                        if (probe_wait > IKCP_PROBE_LIMIT)
                            probe_wait = IKCP_PROBE_LIMIT;
                        ts_probe = current + probe_wait;
                        probe |= IKCP_ASK_SEND;
                    }
                }
            }
            else
            {
                ts_probe = 0;
                probe_wait = 0;
            }

            // flush window probing commands
            if ((probe & IKCP_ASK_SEND) != 0)
            {
                seg.cmd = IKCP_CMD_WASK;
                if (offset + IKCP_OVERHEAD > (int)mtu)
                {
                    output(buffer, offset);
                    //Array.Clear(buffer, 0, offset);
                    offset = 0;
                }
                offset += seg.encode(buffer, offset);
            }

            probe = 0;

            // calculate window size
            var cwnd_ = _imin_(snd_wnd, rmt_wnd);
            if (0 == nocwnd)
                cwnd_ = _imin_(cwnd, cwnd_);

            count = 0;
            int nLen = snd_queue.Count;
            QueueNode<Segment> node = null;
            Segment segment = null;
            for (var k = 0; k < nLen; k++)
            {
                if (_itimediff(snd_nxt, snd_una + cwnd_) >= 0) break;

                node = snd_queue[k];
                segment = node.item;
                // var newseg = snd_queue[k];
                segment.conv = conv;
                segment.cmd = IKCP_CMD_PUSH;
                segment.wnd = seg.wnd;
                segment.ts = current_;
                segment.sn = snd_nxt;
                segment.una = rcv_nxt;
                segment.resendts = current_;
                segment.rto = rx_rto;
                segment.fastack = 0;
                segment.xmit = 0;
                //snd_buf = append<Segment>(snd_buf, newseg);
                snd_buf.Add(node);
                snd_nxt++;
                count++;
            }

            if (0 < count)
            {
                //snd_queue = slice<Segment>(snd_queue, count, snd_queue.Length);
                snd_queue.RemoveRange(0, count);
            }

            // calculate resent
            var resent = (UInt32)fastresend;
            if (fastresend <= 0) resent = 0xffffffff;
            var rtomin = rx_rto >> 3;
            if (nodelay != 0) rtomin = 0;

            // flush data segments
            nLen = snd_buf.Count;
            //foreach (var segment in snd_buf) {
            for (int i = 0; i < nLen; ++i)
            {
                node = snd_buf[i];
                segment = node.item;
                var needsend = false;
                _itimediff(current_, segment.resendts);
                if (0 == segment.xmit)
                {
                    needsend = true;
                    segment.xmit++;
                    segment.rto = rx_rto;
                    segment.resendts = current_ + segment.rto + rtomin;
                }
                else if (_itimediff(current_, segment.resendts) >= 0)
                {
                    needsend = true;
                    segment.xmit++;
                    xmit++;
                    if (0 == nodelay)
                        segment.rto += rx_rto;
                    else
                        segment.rto += rx_rto / 2;
                    segment.resendts = current_ + segment.rto;
                    lost = 1;
                }
                else if (segment.fastack >= resent)
                {
                    needsend = true;
                    segment.xmit++;
                    segment.fastack = 0;
                    segment.resendts = current_ + segment.rto;
                    change++;
                }

                if (needsend)
                {
                    segment.ts = current_;
                    segment.wnd = seg.wnd;
                    segment.una = rcv_nxt;

                    var need = IKCP_OVERHEAD + segment.data.item.nLen;
                    if (offset + need > mtu)
                    {
                        output(buffer, offset);
                        //Array.Clear(buffer, 0, offset);
                        offset = 0;
                    }

                    offset += segment.encode(buffer, offset);
                    if (segment.data.item.nLen > 0)
                    {
                        Array.Copy(segment.data.item.data, 0, buffer, offset, segment.data.item.nLen);
                        offset += segment.data.item.nLen;
                    }

                    if (segment.xmit >= dead_link)
                    {
                        //state = 0;
                    }
                }
            }

            // flash remain segments
            if (offset > 0)
            {
                output(buffer, offset);
                //Array.Clear(buffer, 0, offset);
                offset = 0;
            }

            // update ssthresh
            if (change != 0)
            {
                var inflight = snd_nxt - snd_una;
                ssthresh = inflight / 2;
                if (ssthresh < IKCP_THRESH_MIN)
                    ssthresh = IKCP_THRESH_MIN;
                cwnd = ssthresh + resent;
                incr = cwnd * mss;
            }

            if (lost != 0)
            {
                ssthresh = cwnd / 2;
                if (ssthresh < IKCP_THRESH_MIN)
                    ssthresh = IKCP_THRESH_MIN;
                cwnd = 1;
                incr = mss;
            }

            if (cwnd < 1)
            {
                cwnd = 1;
                incr = mss;
            }
        }

        // update state (call it repeatedly, every 10ms-100ms), or you can ask
        // ikcp_check when to call it again (without ikcp_input/_send calling).
        // 'current' - current timestamp in millisec.
        public void Update(UInt32 current_)
        {

            current = current_;

            if (0 == updated)
            {
                updated = 1;
                ts_flush = current;
            }

            var slap = _itimediff(current, ts_flush);

            if (slap >= 10000 || slap < -10000)
            {
                ts_flush = current;
                slap = 0;
            }

            if (slap >= 0)
            {
                ts_flush += interval;
                if (_itimediff(current, ts_flush) >= 0)
                    ts_flush = current + interval;
                flush();
            }
        }

        // Determine when should you invoke ikcp_update:
        // returns when you should invoke ikcp_update in millisec, if there
        // is no ikcp_input/_send calling. you can call ikcp_update in that
        // time, instead of call update repeatly.
        // Important to reduce unnacessary ikcp_update invoking. use it to
        // schedule ikcp_update (eg. implementing an epoll-like mechanism,
        // or optimize ikcp_update when handling massive kcp connections)
        public UInt32 Check(UInt32 current_)
        {

            if (0 == updated) return current_;

            var ts_flush_ = ts_flush;
            var tm_flush_ = 0x7fffffff;
            var tm_packet = 0x7fffffff;
            var minimal = 0;

            if (_itimediff(current_, ts_flush_) >= 10000 || _itimediff(current_, ts_flush_) < -10000)
            {
                ts_flush_ = current_;
            }

            if (_itimediff(current_, ts_flush_) >= 0) return current_;

            tm_flush_ = (int)_itimediff(ts_flush_, current_);

            QueueNode<Segment> node = null;


            int nCount = snd_buf.Count;
            for (int i = 0; i < nCount; ++i)
            {
                node = snd_buf[i];
                Segment seg = node.item;
                var diff = _itimediff(seg.resendts, current_);
                if (diff <= 0) return current_;
                if (diff < tm_packet) tm_packet = (int)diff;
            }

            /*
        foreach (var seg in snd_buf) {
            var diff = _itimediff(seg.resendts, current_);
            if (diff <= 0) return current_;
            if (diff < tm_packet) tm_packet = (int)diff;
        }
        */

            minimal = (int)tm_packet;
            if (tm_packet >= tm_flush_) minimal = (int)tm_flush_;
            if (minimal >= interval) minimal = (int)interval;

            return current_ + (UInt32)minimal;
        }

        // change MTU size, default is 1400
        public int SetMtu(Int32 mtu_)
        {
            if (mtu_ < 50 || mtu_ < (Int32)IKCP_OVERHEAD) return -1;

            var buffer_ = new byte[(mtu_ + IKCP_OVERHEAD) * 3];
            if (null == buffer_) return -2;

            mtu = (UInt32)mtu_;
            mss = mtu - IKCP_OVERHEAD;
            buffer = buffer_;
            return 0;
        }

        public int Interval(Int32 interval_)
        {
            if (interval_ > 5000)
            {
                interval_ = 5000;
            }
            else if (interval_ < 10)
            {
                interval_ = 10;
            }
            interval = (UInt32)interval_;
            return 0;
        }

        // fastest: ikcp_nodelay(kcp, 1, 20, 2, 1)
        // nodelay: 0:disable(default), 1:enable
        // interval: internal update timer interval in millisec, default is 100ms
        // resend: 0:disable fast resend(default), 1:enable fast resend
        // nc: 0:normal congestion control(default), 1:disable congestion control
        public int NoDelay(int nodelay_, int interval_, int resend_, int nc_)
        {

            if (nodelay_ > 0)
            {
                nodelay = (UInt32)nodelay_;
                if (nodelay_ != 0)
                    rx_minrto = IKCP_RTO_NDL;
                else
                    rx_minrto = IKCP_RTO_MIN;
            }

            if (interval_ >= 0)
            {
                if (interval_ > 5000)
                {
                    interval_ = 5000;
                }
                else if (interval_ < 10)
                {
                    interval_ = 10;
                }
                interval = (UInt32)interval_;
            }

            if (resend_ >= 0) fastresend = resend_;

            if (nc_ >= 0) nocwnd = nc_;

            return 0;
        }

        // set maximum window size: sndwnd=32, rcvwnd=32 by default
        public int WndSize(int sndwnd, int rcvwnd)
        {
            if (sndwnd > 0)
                snd_wnd = (UInt32)sndwnd;

            if (rcvwnd > 0)
                rcv_wnd = (UInt32)rcvwnd;
            return 0;
        }

        // get how many packet is waiting to be sent
        public int WaitSnd()
        {
            return snd_buf.Count + snd_queue.Count;
        }
    }
}
