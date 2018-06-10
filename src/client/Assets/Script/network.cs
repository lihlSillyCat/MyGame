using System.Collections;
using System.Collections.Generic;
using System.Net;
using War;
using War.Base;

//ui to net base
public class netdemo : ISocketSink {

	bool connected_ = false;

	ISocketClient client_ = new TCPSocketClient();

	private Queue<string> msg_ =  new Queue<string>();
	private readonly object lockobj_ = new object();

	public netdemo()
	{
		client_.SetSink (this);
	}

	//接收数据
	public void OnReceive(uint nSocketID, byte[] buffer, int nOffset,int size) 
	{
		string msg = string.Format ("recv msg : [socketid:%d], [buffer:%s], [offset:%d], [size:%d]",
			             nSocketID, buffer, nOffset, size);
		AddViewMsg (msg);
	}

	//连接成功
	public void OnConnect(uint nSocketID)
	{
		connected_ = true;
		AddViewMsg ("connect server succeed");
	}

	//关闭
	public void OnClose(uint nSocketID)
	{
	}


	//连接失败
	public void OnConnectError(uint nSocketID, SOCKET_ERROR eReason)
	{
		connected_ = false;
		AddViewMsg ("connect server failed");
	}


	//重新连接
	public void OnReconnect(uint nSocketID)
	{
	}


	//发送一个数据包完毕
	public void OnFinishSendData(QueueNode<IPackageData> node)
	{
		
	}



	public void ConnectServer(string ip, int port)
	{
		if (connected_) {
			return;
		}
		client_.CreateConnection (ip, port);
	}

	public void Shutdown()
	{
		if (connected_) {
		
			client_.Disconnect ();
		}
	}

	void AddViewMsg(string msg)
	{
		lock (lockobj_) {
			msg_.Enqueue (msg);
		}
	}

	public string GetViewMsg()
	{
		string str = "";
		lock (lockobj_) {
			if (msg_.Count > 0)
				str = msg_.Dequeue ();
		}
		return str;
	}
}
