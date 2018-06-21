/*
author:  袁森峰
date:    2017.11.25
ver:     1.0
desc:    HTTP请求文件  
*/


using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using XLua;
using War.Base;
using UnityEngine.Networking;

namespace War.Script
{
    [LuaCallCSharp]
    public class HTTPReq: MonoBehaviour
    {
        IEnumerator Req(string url)
        {
            var www = new WWW(url);
            yield return www;
            if(www.isDone)
            {
                Debug.Log("完成");
                if (string.IsNullOrEmpty(www.error))
                    Debug.Log("成功");
                else
                    Debug.Log("失败"+www.error);
            }
            else
            {
                Debug.Log("未完成");
            }

        }

        public void SendHttpReq(string url)
        {       
            StartCoroutine(Req(url));
        }
    }
}