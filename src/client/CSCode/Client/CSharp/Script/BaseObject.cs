using UnityEngine;
using System.Collections.Generic;
using XLua;

namespace War.Script
{
    [LuaCallCSharp]
    public class BaseObject
    {
        protected bool m_IsDestroyed = false;
        protected bool m_IsUpdating = false;

        #region timer
        protected LuaFunction m_FixedUpdateTimer;
        protected LuaFunction m_UpdateTimer;
        protected LuaFunction m_LateUpdateTimer;
        #endregion

        public BaseObject()
        {
            ObjectManager.Instance.OnObjectCreate(this);
        }

        #region updates
        [BlackListAttribute]
        public virtual void FixedUpdate(float fixedDeltaTime)
        {
            m_IsUpdating = true;
            if (m_FixedUpdateTimer != null)
            {
                m_FixedUpdateTimer.Action(fixedDeltaTime);
            }
            m_IsUpdating = false;

            if (m_IsDestroyed)
            {
                ClearAllUpdateTimers();
            }
        }

        [BlackListAttribute]
        public virtual void Update(float deltaTime)
        {
            m_IsUpdating = true;
            if (m_UpdateTimer != null)
            {
                m_UpdateTimer.Action(deltaTime);
            }
            m_IsUpdating = false;

            if (m_IsDestroyed)
            {
                ClearAllUpdateTimers();
            }
        }

        [BlackListAttribute]
        public virtual void LateUpdate()
        {
            m_IsUpdating = true;
            if (m_LateUpdateTimer != null)
            {
                m_LateUpdateTimer.Call();
            }
            m_IsUpdating = false;

            if (m_IsDestroyed)
            {
                ClearAllUpdateTimers();
            }
        }
        #endregion

        #region timer schedule

        public void SetFixedUpdateTimer(LuaFunction func)
        {
            if (m_LateUpdateTimer != null)
            {
                m_LateUpdateTimer.Dispose();
            }
            m_FixedUpdateTimer = func;
        }

        public void SetUpdateTimer(LuaFunction func)
        {
            if (m_UpdateTimer != null)
            {
                m_UpdateTimer.Dispose();
            }
            m_UpdateTimer = func;
        }

        public void AddLateUpdateTimer(LuaFunction func)
        {
            if (m_LateUpdateTimer != null)
            {
                m_LateUpdateTimer.Dispose();
            }
            m_LateUpdateTimer = func;
        }

        #endregion

        protected void ClearAllUpdateTimers()
        {
            if (m_LateUpdateTimer != null)
            {
                m_LateUpdateTimer.Dispose();
                m_LateUpdateTimer = null;
            }

            if (m_UpdateTimer != null)
            {
                m_UpdateTimer.Dispose();
                m_UpdateTimer = null;
            }

            if (m_FixedUpdateTimer != null)
            {
                m_FixedUpdateTimer.Dispose();
                m_FixedUpdateTimer = null;
            }
        }

        public virtual void Dispose()
        {
            if (!m_IsUpdating)
            {
                ClearAllUpdateTimers();
            }

            ObjectManager.Instance.OnObjectDestroy(this);
            m_IsDestroyed = true;
        }
    }
}

namespace XLua
{
#if USE_UNI_LUA
    using LuaAPI = UniLua.Lua;
    using RealStatePtr = UniLua.ILuaState;
    using LuaCSFunction = UniLua.CSharpFunctionDelegate;
#else
    using LuaAPI = XLua.LuaDLL.Lua;
    using RealStatePtr = System.IntPtr;
    using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif

    using System;

    public partial class LuaFunction : LuaBase
    {
        public void Action(float a)
        {
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (luaEnv.luaEnvLock)
            {
#endif
            var L = luaEnv.L;
            var translator = luaEnv.translator;
            int oldTop = LuaAPI.lua_gettop(L);
            int errFunc = LuaAPI.load_error_func(L, luaEnv.errorFuncRef);
            LuaAPI.lua_getref(L, luaReference);
            LuaAPI.lua_pushnumber(L, a);
            int error = LuaAPI.lua_pcall(L, 1, 0, errFunc);
            if (error != 0)
                luaEnv.ThrowExceptionFromError(oldTop);
            LuaAPI.lua_settop(L, oldTop);
#if THREAD_SAFE || HOTFIX_ENABLE
            }
#endif
        }
    }
}