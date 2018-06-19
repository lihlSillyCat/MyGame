/*
author:  袁森峰
date:    2018.01.08
ver:     3.0
desc:    获取公告的json字符串  
*/
using UnityEngine;
using System;
using System.Collections;
using XLua;

namespace War.Script
{
    [LuaCallCSharp]
    public class Notice : MonoBehaviour
    {
        protected Action<string> m_ReqGetJsonAction;
        protected LuaFunction m_LuaGetJsonCallback;

        IEnumerator ReqGetJson(string url)
        {
            var www = new WWW(url);
            yield return www;

            if (www.isDone)
            {
                string szJsonData = System.Text.Encoding.UTF8.GetString(www.bytes);
                m_ReqGetJsonAction(szJsonData);
                m_LuaGetJsonCallback.Dispose();
            }
            www.Dispose();
            www = null;
        }

        public void GetJson(string url, LuaFunction callback)
        {
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogError("url为空");
                return;
            }

            //不是同一个callback，释放掉
            if (callback != m_LuaGetJsonCallback || false == callback.Equals(m_LuaGetJsonCallback))
            {
                if (m_LuaGetJsonCallback != null)
                {
                    m_LuaGetJsonCallback.Dispose();
                }
                m_LuaGetJsonCallback = callback;

                m_ReqGetJsonAction = delegate (string content)
                {
                    m_LuaGetJsonCallback.Action<string>(content);
                };

            }

            StartCoroutine(ReqGetJson(url));
        }
    }
}