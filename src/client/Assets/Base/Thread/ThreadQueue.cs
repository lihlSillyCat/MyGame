/*******************************************************************
** 文件名:	Queue.cs
** 版  权:	(C) 冰川网络有限公司
** 创建人:	许德纪
** 日  期:	2017.12.02
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
    public class ThreadQueue<T>
    {

        //当前队列节点
        QueueNode<T> m_oHeadNode = null;
        QueueNode<T> m_oTailNode = null;


        //压入的个数
        int m_nPushCount;

        //弹出个数
        int m_nPopCount;


        public bool Create()
        {

            m_oHeadNode = new QueueNode<T>();
            m_oTailNode = m_oHeadNode;

            return true;
        }
        //是否节点为空
        public bool IsEmpty()
        {
            return null == m_oHeadNode.next;
        }

        //释放 
        public void Release()
        {
            QueueNode<T> node = null;
            do
            {
                node = Pop();
            } while (node != null);
        }

        //弹出队列头数据
        public QueueNode<T> Pop()
        {
            if(null != m_oHeadNode.next)
            {
                m_nPopCount++;
                QueueNode<T> h = m_oHeadNode;
                m_oHeadNode = h.next;
                h.next = null;
                return h;

            }

            return null;

        }

        //压入数据节点
       public void Push(QueueNode<T> n)
        {
            ++m_nPushCount;

           
            m_oTailNode.item = n.item;
            n.next = null;
            m_oTailNode.next = n;
            m_oTailNode = n;



        }

        //取得缓存的个数
        public int Size()
        {
            return m_nPushCount - m_nPopCount;
        }


    }
}
