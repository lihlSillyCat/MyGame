using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Login : MonoBehaviour, ILoginNetworkHandler {

	public Text ip_;
	public Text port_;
	public Text name_;
	public Text psd_;
	public Text msg_;

    LoginNetworkDriver netdriver_ = null;

    // Use this for initialization
    void Start () {
        netdriver_ = new LoginNetworkDriver(this);
    }

    //接收数据
    public void OnRecvMsg(string msg)
    {
		Debug.Log("recv net msg : " + msg);
		msg_.text = "recv net msg : " + msg;
    }

    //连接回调
    public void OnConnected(bool success, string msg)
    {

    }

    //模块消息回调
    public void OnRunningMsg(string msg)
    {
        Debug.Log(msg);
        msg_.text = msg;
    }

    // Update is called once per frame
    void Update () {
        netdriver_.Update();
	}

	void OnDestroy()
	{
		netdriver_.Shutdown ();
	}

	public void OnBtnDisconnect()
	{
        netdriver_.Shutdown ();
	}

	//demo for login
	public void OnBtnLogin()
	{
		if (string.IsNullOrEmpty (ip_.text)) 
		{
			Debug.LogWarning ("please input ip...");
			return;
		}
		if (string.IsNullOrEmpty (port_.text)) 
		{
			Debug.LogWarning("please input port...");
			return;
		}
		if (string.IsNullOrEmpty (name_.text)) 
		{
			Debug.LogWarning ("please input name...");
			return;
		}
		if (string.IsNullOrEmpty (psd_.text)) 
		{
			Debug.LogWarning ("please input password...");
			return;
		}

        //connect server
        netdriver_.ConnectServer(ip_.text, System.Convert.ToInt32(port_.text));

		//do shakehands

		//login

	}
}
