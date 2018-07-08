/*******************************************************************
** 文件名:	LuaMsgHandler.cs
** 版  权:	(C) 冰川网络有限公司
** 创建人:	许德纪
** 日  期:	2018.02.27
** 版  本:	1.0
** 描  述:	
** 应  用:  lua接收消息转发器

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/
using UnityEngine;
using System;
using XLua;
using LuaAPI = XLua.LuaDLL.Lua;
using RealStatePtr = System.IntPtr;

namespace War.Script
{
   public class LuaMsgHandler : IMsgHandler
    {
        //lua环境
        private LuaEnv m_luaEnv;

        //错误函数索引
        private  int m_errorFuncRef;

        //回调函数索引
        private int m_callbackFuncRef;

        //创建
        public bool Create(object env, string callback)
        {
            m_luaEnv = env as LuaEnv;
            m_errorFuncRef = m_luaEnv.errorFuncRef;
            RealStatePtr L = m_luaEnv.rawL;
            int nTop = LuaAPI.lua_gettop(L);
            int nRef = LuaAPI.xlua_getglobal(L, callback);
 
            m_callbackFuncRef = LuaAPI.luaL_ref(L);
            LuaAPI.lua_settop(L,nTop);

            return true;

        }

        //销毁
        public void Dispose()
        {
            m_luaEnv = null;
        }
        
        //包处理函数
        public void OnHandler(
          byte srcEndpoint,
          byte dstEndpoint,
          UInt16 keyModule,
          UInt32 keyAction,
          UInt32 sid,
          byte[] buffer, int nLen)
        {

            RealStatePtr L = m_luaEnv.rawL;
            int err_func = LuaAPI.load_error_func(L, m_errorFuncRef);


            LuaAPI.lua_getref(L, m_callbackFuncRef);

            LuaAPI.xlua_pushinteger(L, srcEndpoint);
            LuaAPI.xlua_pushinteger(L, dstEndpoint);
            LuaAPI.xlua_pushinteger(L, keyModule);
            LuaAPI.xlua_pushuint(L, keyAction);
            LuaAPI.xlua_pushuint(L, sid);
            LuaAPI.xlua_pushlstring(L, buffer, nLen);
        

            int __gen_error = LuaAPI.lua_pcall(L, 6, 0, err_func);
            if (__gen_error != 0)
                m_luaEnv.ThrowExceptionFromError(err_func - 1);

            LuaAPI.lua_settop(L, err_func - 1);

        }

        public void OnHandler(byte serverID, ushort msgID, byte[] buffer, int nLen)
        {
            RealStatePtr L = m_luaEnv.rawL;
            int err_func = LuaAPI.load_error_func(L, m_errorFuncRef);
            LuaAPI.lua_getref(L, m_callbackFuncRef);
            LuaAPI.xlua_pushinteger(L, serverID);
            LuaAPI.xlua_pushinteger(L, msgID);
            LuaAPI.xlua_pushlstring(L, buffer, nLen);
            int __gen_error = LuaAPI.lua_pcall(L, 6, 0, err_func);
            if (__gen_error != 0)
                m_luaEnv.ThrowExceptionFromError(err_func - 1);
            LuaAPI.lua_settop(L, err_func - 1);
        }
    }
}
