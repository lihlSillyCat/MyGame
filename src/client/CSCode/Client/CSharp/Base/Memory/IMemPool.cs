/*******************************************************************
** 文件名:	MemPool.cs
** 版  权:	(C) 冰川网络有限公司
** 创建人:	许德纪
** 日  期:	2017.12.04
** 版  本:	1.0
** 描  述:	
** 应  用:   内存池接口

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
    public interface IMemPool
    {

        //创建
        void Create();

        //销毁
        void Release();

        //分配节点
         QueueNode<ByteData> Aloc(int nSize);


        //回收节点
        void Recycle(ref QueueNode<ByteData> node);

        //设置缓存队列最大数量
        void SetMaxQueueSize(int nSize);
    }
}
