/* 登陆作为一个单独的服务器节点对客户端服务器。不影响其他玩家游戏体验
 * 
 * 基于文本协议。这样做的原因是登陆节点可以由 Nginx 框架来做，具体高性能，高爆发，防攻击等特点。
 * 登陆握手流程为一个标准的成熟方案。
 * --[[

Protocol:

	line (\n) based text protocol

	1. Server->Client : base64(8bytes random challenge)
	2. Client->Server : base64(8bytes handshake client key)
	3. Server: Gen a 8bytes handshake server key
	4. Server->Client : base64(DH-Exchange(server key))
	5. Server/Client secret := DH-Secret(client key/server key)
	6. Client->Server : base64(HMAC(challenge, secret))
	7. Client->Server : DES(secret, base64(token))
	8. Server : call auth_handler(token) -> server, uid (A user defined method)
	9. Server : call login_handler(server, uid, secret) ->subid (A user defined method)
	10. Server->Client : 200 base64(subid)

Error Code:
	400 Bad Request . challenge failed
	401 Unauthorized . unauthorized by auth_handler
	403 Forbidden . login_handler failed
	406 Not Acceptable . already in login (disallow multi login)

Success:
	200 base64(subid)
]]
 *  
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

//登陆模块网络回调
public interface ILoginNetworkHandler
{
    //接收数据
    void OnRecvMsg(string msg);

    //连接回调
    void OnConnected(bool success, string msg);

    //模块消息回调
    void OnRunningMsg(string msg);
}

//登陆模块网络驱动
public class LoginNetworkDriver {

    //网络参数
    const int kMaxNetSize = 2048;

    private Socket socket_;
    //本模块网络基于文本协议，消息队列内容为字符串
    private Queue<string> msg_queue_send_ = new Queue<string>();
    private Queue<string> msg_queue_recv_ = new Queue<string>();
    private Queue<string> msg_queue_view_ = new Queue<string>();
    //多线程锁
    private readonly object msg_view_lock_ = new object();
    private readonly object msg_recv_lock_ = new object();
    private readonly object msg_send_lock_ = new object();
    //多线程
    private Thread recv_thread_;
    private Thread send_thread_;
    private bool recv_thread_run_ = false;
    private bool send_thread_run_ = false;

    //回调
    ILoginNetworkHandler handler_ = null;
    bool notify_connect_ = false;

    //构造函数
    public LoginNetworkDriver(ILoginNetworkHandler handler)
    {
        Debug.Assert(handler != null);
        handler_ = handler;
    }

    //连接服务器
    public void ConnectServer(string ip, int port)
    {
        if (socket_ != null && socket_.Connected)
        {
            Shutdown();
        }

        IPAddress[] address = Dns.GetHostAddresses(ip);
        AddressFamily addressFamily = AddressFamily.InterNetwork;
        if (address[0].AddressFamily == AddressFamily.InterNetworkV6)
        {
            addressFamily = AddressFamily.InterNetworkV6;
        }
        socket_ = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp);
        socket_.NoDelay = true;

        socket_.BeginConnect(address, port, new AsyncCallback(ConnectHandler), socket_);
    }

    //驱动模块运转
    public void Update()
    {
        string str = "";

        //提取网络消息
        lock(msg_recv_lock_)
        {
            if (msg_queue_recv_.Count > 0)
            {
                str = msg_queue_recv_.Dequeue();
            }
        }
        if (!String.IsNullOrEmpty(str))
        {
            handler_.OnRecvMsg(str);
            str = "";    
        }

        //连接消息
        if (notify_connect_)
        {
            handler_.OnConnected(socket_ != null && socket_.Connected, "");
            notify_connect_ = false;
        }

        //提取模块消息
        lock (msg_view_lock_)
        {
            if (msg_queue_view_.Count > 0)
            {
                str = msg_queue_view_.Dequeue();
            }
        }
        if (!String.IsNullOrEmpty(str))
        {
            handler_.OnRunningMsg(str);
            str = "";
        }
    }

    //发送消息
    public void Send(string msg)
    {
        if (socket_ == null || !socket_.Connected)
        {
            return;
        }

        lock(msg_send_lock_)
        {
            msg_queue_send_.Enqueue(msg);
        }
    }

    //停止模块
    public void Shutdown()
    {
        recv_thread_run_ = false;
        send_thread_run_ = false;
        if (socket_ != null && socket_.Connected)
        {
            socket_.Shutdown(SocketShutdown.Both);
        }
		recv_thread_.Join ();
		send_thread_.Join ();

		AddViewMsg ("network shutdown");
    }

    //连接回调
    private void ConnectHandler(IAsyncResult iar)
    {
        try
        {
            socket_.EndConnect(iar);

			notify_connect_ = true;
			AddViewMsg("Connect server succeed. Start recv and send thread.");

            recv_thread_ = new Thread(new ThreadStart(RecvThreadMain));
            send_thread_ = new Thread(new ThreadStart(SendThreadMain));
            recv_thread_run_ = true;
            send_thread_run_ = true;
            recv_thread_.Start();
            send_thread_.Start();
        }
        catch (SocketException)
        {
            notify_connect_ = true;
            AddViewMsg("Connect server failed.");
            Shutdown();
        }
    }

    //网络接收线程
    void RecvThreadMain()
    {
        byte[] buf = new byte[kMaxNetSize];
        int buf_len = 0, recv_len = 0;
        while (recv_thread_run_)
        {
            recv_len = socket_.Receive(buf, buf_len, kMaxNetSize - buf_len, SocketFlags.None);
            bool res = false;
            //提取一行
            for (int i = buf_len; i < buf_len + recv_len; i++)
            {
                if (buf[i] == '\n')
                {
                    //完整消息则入队
                    string line = System.Text.Encoding.Default.GetString(buf, 0, i);
                    lock (msg_recv_lock_)
                    {
                        msg_queue_recv_.Enqueue(line);
                    }

                    //剩余字节存放在容器中
                    int remain_len = buf_len + recv_len - i - 1;
                    if (remain_len > 0)
                    {
                        Buffer.BlockCopy(buf, i + 1, buf, 0, remain_len);
                        buf_len = remain_len;
                    }
                    res = true;
                    break;
                }
            }
            if (!res)
            {
                buf_len += recv_len;
            }
        }
    }

    //发送消息线程
    void SendThreadMain()
    {
        SocketError error;
        //byte[] buf = new byte[kMaxNetSize], buftemp;
        string strbuf = "";
        bool has_msg = false;
        while (send_thread_run_)
        {
            lock (msg_send_lock_)
            {
                if (msg_queue_send_.Count > 0)
                {
                    strbuf = msg_queue_send_.Dequeue();
                    has_msg = true;
                }
            }
            if (!has_msg)
            {
                Thread.Sleep(10);
                continue;
            }
			byte[] buftemp = System.Text.Encoding.Default.GetBytes(strbuf + '\n');
			socket_.Send(buftemp, 0, buftemp.Length, SocketFlags.None, out error);

            if (error != SocketError.Success)
            {
                AddViewMsg(string.Format("Send message error:{0}", error.ToString()));
            }
        }
    }

    void AddViewMsg(string msg)
    {
        lock (msg_view_lock_)
        {
            msg_queue_view_.Enqueue(msg);
        }
    }
}
