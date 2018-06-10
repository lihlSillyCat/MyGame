/*******************************************************************
** 文件名:	ISocketClient.cs
** 版  权:	(C) 冰川网络有限公司
** 创建人:	许德纪
** 日  期:	2017.12.16
** 版  本:	1.0
** 描  述:	
** 应  用:  基础接口文件

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
    public interface ISocketClient
    {
       

        //创建连接
        void CreateConnection(string serverAddress, int port);

        //设置上层回调
        void SetSink(ISocketSink sink);

        //设置连接的ID唯一标识连接号
        void SetSocketID(uint nSocketID);

        //发送数据包
        void Send(QueueNode<IPackageData> node);

        //断开连接
        void Disconnect();

        //关闭
        void Close();

        //是否可以关闭
        bool CanClose();

        //推动更新
        void Update();

    }
}
