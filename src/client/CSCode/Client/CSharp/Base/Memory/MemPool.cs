/*******************************************************************
** 文件名:	MemPool.cs
** 版  权:	(C) 冰川网络有限公司
** 创建人:	许德纪
** 日  期:	2017.12.03
** 版  本:	1.0
** 描  述:	
** 应  用:  内存池对象

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace War.Base
{
    public class MemPool: IMemPool
    {

        //对象池
        private Dictionary<int, ThreadQueue<ByteData>> m_dicMemPool;


        //最小对象大小
        private const int MIN_SIZE = 8;

        //最大对象大小
        private const int MAX_SIZE = 1024;

        //设置队列最大的缓存个数
        private int m_nQueueMaxSize = 30;

        //幂的边界
        private int[] m_aryPowerBounds = { MIN_SIZE, 16,32,64,128,256,512, MAX_SIZE };

        //创建
        public void Create()
        {

            
            m_dicMemPool = new Dictionary<int, ThreadQueue<ByteData>>();
            ThreadQueue<ByteData> threadQueue = null;


            for (int nPower = MIN_SIZE; nPower<= MAX_SIZE; nPower = nPower<<1)
            {
                threadQueue = new ThreadQueue<ByteData>();
                threadQueue.Create();
                m_dicMemPool.Add(nPower, threadQueue);
       

            }


        }

        public void Release()
        {
            foreach(ThreadQueue<ByteData> queue in m_dicMemPool.Values)
            {
                queue.Release();
            }

            m_dicMemPool = null;

        }

        //分配对象节点
        public  QueueNode<ByteData> Aloc(int nSize)
        {
            QueueNode<ByteData> node = null;
            int nBoundSize = GetBoundSize(nSize);
            if (nBoundSize>0)
            {
                ThreadQueue<ByteData> queueData = null;
                if(m_dicMemPool.TryGetValue(nBoundSize, out queueData))
                {
                    node = queueData.Pop();
                    if(null!=node)
                    {
                        return node;
                    }
                }

            }else
            {
                nBoundSize = nSize;
            }

            node = new QueueNode<ByteData>();
            node.item = new ByteData();
            node.next = null;
            node.item.data = new byte[nBoundSize];
            node.item.nLen = 0;
            return node;


        }

        //回收对象节点
        public void Recycle(ref QueueNode<ByteData> node)
        {
            int nBoundSize = node.item.data.Length;
            ThreadQueue<ByteData> queueData = null;
            if (m_dicMemPool.TryGetValue(nBoundSize, out queueData))
            {
                if(queueData.Size()< m_nQueueMaxSize)
                {
                    queueData.Push(node);
                }
               
            }
        }

        //取得边界幂值
        public int GetBoundSize(int nSize)
        {
            int nH = m_aryPowerBounds.Length;
            int nM = nH / 2;
            int nL = 0;
            if(nSize> m_aryPowerBounds[nM])
            {
                nL = nM + 1;
            }else
            {
                nH = nM+1;
            }

            for(int i= nL; i<nH;++i)
            {
                if(nSize < m_aryPowerBounds[i])
                {
                    return m_aryPowerBounds[i];
                }
            }

            return 0;


        }

        //设置队列的最大个数
        public void SetMaxQueueSize(int nSize)
        {
            m_nQueueMaxSize = nSize;
        }


    }
}
