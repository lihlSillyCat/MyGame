/*******************************************************************
** 文件名:	QueueNode.cs
** 版  权:	(C) 冰川网络有限公司
** 创建人:	许德纪
** 日  期:	2017.12.03
** 版  本:	1.0
** 描  述:	
** 应  用:  线程访问队列节点

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
    public class QueueNode<T>
    {
        //节点数据
        public T item;

        //节点下一个连接
        public QueueNode<T> next = null;

    }
}
