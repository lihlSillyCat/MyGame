/*******************************************************************
** 文件名:	ThreadDataSwap.cs
** 版  权:	(C) 冰川网络有限公司
** 创建人:	许德纪
** 日  期:	2017.12.14
** 版  本:	1.0
** 描  述:	
** 应  用:  支持两个线程的读写队列,一个线程读，一个线程写

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace War.Base
{
    class ThreadDataSwap
    {

        //发送相关
        private IMemPool m_oMemPool;
        private ThreadQueue<ByteData> m_oDataQueue;
        private ThreadQueue<ByteData> m_oRecycleQueue;

        //创建
        public bool Create()
        {
            m_oMemPool = new MemPool();
            m_oMemPool.Create();

            m_oDataQueue = new ThreadQueue<ByteData>();
            m_oDataQueue.Create();
            m_oRecycleQueue = new ThreadQueue<ByteData>();
            m_oRecycleQueue.Create();


            return true;
        }

        //销毁
        public void Release()
        {
            if (null != m_oMemPool)
            {
                m_oMemPool.Release();
                m_oMemPool = null;
            }

            if (null != m_oDataQueue)
            {
                m_oDataQueue.Release();
                m_oDataQueue = null;
            }

            if (null != m_oRecycleQueue)
            {
                m_oRecycleQueue.Release();
                m_oRecycleQueue = null;
            }

        }

        //分配节点(在生产者线程调用)
        public QueueNode<ByteData> Aloc(int nSize)
        {
            return m_oMemPool.Aloc(nSize);
        }

        //回收节点(在消费者线程调用)
        public void Recycle(ref QueueNode<ByteData> node)
        {
            m_oRecycleQueue.Push(node);
        }

        //弹出节点(在消费者中线程调用)
        public QueueNode<ByteData> Pop()
        {
            return m_oDataQueue.Pop();
        }

        //压入节点(在生产者线程调用)
        public void Push(QueueNode<ByteData> node)
        {
            m_oDataQueue.Push(node);
        }

        //更新(在生产者线程调用)
        public void Update()
        {
            //int nCount = 10;
            QueueNode<ByteData> node = null;
            do
            {
                node = m_oRecycleQueue.Pop();
                if (node != null)
                {
                    m_oMemPool.Recycle(ref node);
                }

                //--nCount;



            } while (node != null);//&& nCount>0);
        }

        //取得当前积压包的数量
        public int DataCount()
        {
            return m_oDataQueue.Size();
        }





        }
}
