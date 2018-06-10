using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class start : MonoBehaviour {

	public Text ip_;
	public Text port_;
	public Text name_;
	public Text psd_;
	public Text msg_;

	private netdemo net_ = new netdemo();

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		string str = net_.GetViewMsg ();
		if (!string.IsNullOrEmpty (str)) {
			Debug.Log (str);
			msg_.text = str;
		}
	}

	void OnDestroy()
	{
	}

	public void OnBtnDisconnect()
	{
		net_.Shutdown ();

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
		net_.ConnectServer(ip_.text, System.Convert.ToInt32(port_.text));

		//do shakehands

		//login

	}
}
