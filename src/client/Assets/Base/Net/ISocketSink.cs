/*******************************************************************
** 文件名:	ISocketSink.cs
** 版  权:	(C) 冰川网络有限公司
** 创建人:	许德纪
** 日  期:	2017.12.16
** 版  本:	1.0
** 描  述:	
** 应  用:  网络层回调

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
    public enum SOCKET_ERROR
    {
        SOCKET_ERROR_NON = 0,
        SOCKET_ERROR_CONNECT_FAIL, //建立连接失败
        SOCKET_ERROR_CONNECT_CLOSE, //连接错误，接收的字节是0
        SOCKET_ERROR_CONNECT_ERROR, //连接错误
        SOCKET_ERROR_NO_CONNECT ,   //网络不通，客户端没有连接
        SOCKET_ERROR_SERVER_DISCONNECT , //服务器主动断开连接
        SOCKET_ERROR_CLIENT_DISCONNECT , //客户端主动断开连接
        SOCKET_ERROR_CLIENT_HANDSHAKE_TIMEOUT , //客户端握手心跳超时,主动断开网络
        SOCKET_ERROR_SERVER_HANDSHAKE_DISCONNECT , //服务器握手心跳超时,通知断开网络
       

        SOCKET_ERROR_MAX ,
    }

    public interface ISocketSink
    {
        //接收数据
        void OnReceive(uint nSocketID, byte[] buffer, int nOffset,int size);

        //连接成功
        void OnConnect(uint nSocketID);

        //关闭
        void OnClose(uint nSocketID);

        //连接失败
        void OnConnectError(uint nSocketID, SOCKET_ERROR eReason);

        //重新连接
       void OnReconnect(uint nSocketID);

        //发送一个数据包完毕
        void OnFinishSendData(QueueNode<IPackageData> node);
    }
}
