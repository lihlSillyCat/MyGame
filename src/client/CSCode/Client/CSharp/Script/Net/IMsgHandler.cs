/*******************************************************************
** 文件名:	IMsgHandler.cs
** 版  权:	(C) 冰川网络有限公司
** 创建人:	许德纪
** 日  期:	2018.02.27
** 版  本:	1.0
** 描  述:	
** 应  用:  网络消息处理器接口

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace War.Script
{
    public interface IMsgHandler
    {
        //创建
        bool Create(object env, string callback);

        //销毁
        void Dispose();

        //处理消息
        void OnHandler(byte srcEndpoint,
          byte dstEndpoint,
          UInt16 keyModule,
          UInt32 keyAction,
          UInt32 sid,
          byte[] buffer,
          int nLen);
    }
}
